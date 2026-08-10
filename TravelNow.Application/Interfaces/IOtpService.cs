namespace TravelNow.Application.Interfaces;

public interface IOtpService
{
    string GenerateOtp();
    string HashOtp(string otp, string salt);
    string GenerateSalt();
    bool VerifyOtp(string otp, string storedHash, string salt);
}