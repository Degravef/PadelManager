namespace Core.Interfaces;

// Marker for entities that need optimistic concurrency control because they're mutated by a
// read-then-write from more than one code path (e.g. a payment vs. the daily lifecycle batch).
// Dal.UnitOfWork increments Version on every Modified entry right before SaveChanges, and the
// matching Dal.Configurations class marks the property IsConcurrencyToken() so EF includes its
// original value in the UPDATE/DELETE's WHERE clause — a stale write then affects zero rows and
// EF raises DbUpdateConcurrencyException instead of silently overwriting a concurrent change.
public interface IConcurrencyToken
{
    int Version { get; set; }
}
