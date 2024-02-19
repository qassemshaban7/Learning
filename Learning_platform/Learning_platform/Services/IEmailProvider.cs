namespace Learning_platform.Services
{
    public interface IEmailProvider
    {
        public Task<int> SendResetCode(string to);
    }
}
