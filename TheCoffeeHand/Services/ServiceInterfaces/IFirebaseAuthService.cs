namespace Services.ServiceInterfaces
{
    public interface IFirebaseAuthService
    {
        Task<string> SignInWithFirebaseAsync(string idToken, string? fcmToken);
        Task<string> SignInWithEmailAndPasswordAsync(string email, string password);
        Task RegisterWithEmailPasswordFireBaseAsync(string email, string password);
        Task<string> SignInWithEmailAndPasswordFirebaseAsync(string email, string password);
        Task DisableUserAsync(string email);
        Task EnableUserAsync(string email);
    }
}
