namespace Core.Interfaces;

public interface IConcurrencyToken
{
    int Version { get; set; }
}
