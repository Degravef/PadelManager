using Bll.Rules;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Core.Dtos;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;

namespace Bll.Services;

public class PaymentService(
    IParticipationRepository participationRepository,
    IMatchRepository matchRepository,
    IMemberRepository memberRepository,
    IBalanceDueRepository balanceDueRepository,
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IPaymentService
{
    // RG-PAY-003/007/008, RG-ETA-001: pays the seat, absorbs any outstanding balance due, and completes
    // the match if all 4 seats are now paid.
    public async Task<PaymentDto> PayParticipationAsync(string matricule, int participationId, PayDto dto)
    {
        Participation participation = await GetParticipationOrThrowAsync(participationId);
        Member member = await GetMemberOrThrowAsync(matricule);
        if (participation.MemberId != member.Id)
            throw new ParticipationNotFoundException(participationId); // ownership non-leak

        Match match = participation.Match!;
        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        if (!MatchModifiableRule.IsModifiable(match, now))
            throw new MatchNotModifiableException();
        if (participation.Status == ParticipationStatus.Paid)
            throw new ParticipationAlreadyPaidException();

        // RG-PAY-007: an outstanding balance due is added to the amount of the next paid registration.
        var unpaidBalances = (await balanceDueRepository.GetOutstandingByMemberIdAsync(member.Id)).ToList();
        decimal balanceAmount = unpaidBalances.Sum(s => s.Amount);

        var payment = new Payment
        {
            MemberId = member.Id,
            ParticipationId = participationId,
            // ASSUMPTION: Payment->BalanceDue is a single FK; when several balances are folded in, only
            // the first is linked for traceability while every outstanding balance still gets marked
            // Paid below.
            BalanceDueId = unpaidBalances.Count > 0 ? unpaidBalances[0].Id : null,
            Amount = participation.AmountDue + balanceAmount,
            PaymentDate = now,
            PaymentMethod = dto.PaymentMethod ?? string.Empty,
            Status = PaymentStatus.Validated
        };
        await paymentRepository.AddAsync(payment);

        foreach (BalanceDue balance in unpaidBalances)
        {
            balance.Status = BalanceDueStatus.Paid;
            balance.SettlementDate = now;
            balanceDueRepository.Update(balance);
        }

        participation.Status = ParticipationStatus.Paid;
        participation.PaymentDate = now;
        participationRepository.Update(participation);

        match.AmountPaid += participation.AmountDue;

        // RG-ETA-001: the match is complete once its 4 seats are paid.
        var otherParticipations = await participationRepository.GetByMatchIdAsync(match.Id);
        int paidCountAfter = RosterCompleteRule.CountPaid(otherParticipations) + 1; // +1: this seat, not yet persisted
        if (paidCountAfter >= 4)
            match.Status = MatchStatus.Complete;

        matchRepository.Update(match);

        await unitOfWork.SaveChangesAsync();

        return ToDto(payment);
    }

    // RG-PAY-005/006: lets a member owing a balance settle their debt directly, without waiting to
    // join another paying match.
    public async Task<PaymentDto> PayBalanceDueAsync(string matricule, int balanceDueId, PayDto dto)
    {
        BalanceDue? balance = await balanceDueRepository.GetByIdAsync(balanceDueId);
        if (balance is null)
            throw new BalanceDueNotFoundException(balanceDueId);

        Member member = await GetMemberOrThrowAsync(matricule);
        if (balance.MemberId != member.Id)
            throw new BalanceDueNotFoundException(balanceDueId); // ownership non-leak

        if (balance.Status == BalanceDueStatus.Paid)
            throw new BalanceDueAlreadyPaidException();

        DateTime now = timeProvider.GetUtcNow().UtcDateTime;
        var payment = new Payment
        {
            MemberId = member.Id,
            BalanceDueId = balanceDueId,
            Amount = balance.Amount,
            PaymentDate = now,
            PaymentMethod = dto.PaymentMethod ?? string.Empty,
            Status = PaymentStatus.Validated
        };
        await paymentRepository.AddAsync(payment);

        balance.Status = BalanceDueStatus.Paid;
        balance.SettlementDate = now;
        balanceDueRepository.Update(balance);

        await unitOfWork.SaveChangesAsync();

        return ToDto(payment);
    }

    // Lets a member discover what they owe (and its id) before calling PayBalanceDueAsync — RG-RES-006
    // blocks new reservations while a balance is outstanding, so a member needs a way to find it.
    public async Task<IEnumerable<BalanceDueDto>> GetMyUnpaidBalancesAsync(string matricule)
    {
        Member member = await GetMemberOrThrowAsync(matricule);
        var balances = await balanceDueRepository.GetOutstandingByMemberIdAsync(member.Id);
        return balances.Select(ToDto);
    }

    private async Task<Participation> GetParticipationOrThrowAsync(int id)
    {
        Participation? participation = await participationRepository.GetByIdAsync(id);
        if (participation is null)
            throw new ParticipationNotFoundException(id);
        return participation;
    }

    private async Task<Member> GetMemberOrThrowAsync(string matricule)
    {
        Member? member = await memberRepository.GetByMatriculeAsync(matricule);
        if (member is null)
            throw new MemberNotFoundByMatriculeException(matricule);
        return member;
    }

    private static PaymentDto ToDto(Payment p) => new(
        p.Id, p.MemberId, p.ParticipationId, p.BalanceDueId, p.Amount, p.PaymentDate, p.Status.ToString());

    private static BalanceDueDto ToDto(BalanceDue s) => new(s.Id, s.MemberId, s.MatchId, s.Amount, s.Status.ToString(), s.CreatedAt);
}
