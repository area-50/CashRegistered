namespace Domain.Security.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password);
    
    bool VerifyHash(string password, string hashedPassword);
}