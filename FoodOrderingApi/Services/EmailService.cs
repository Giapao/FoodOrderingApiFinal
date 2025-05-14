using System.Net.Mail;
using System.Net;

namespace FoodOrderingApi.Services
{
    /// <summary>
    /// Interface định nghĩa phương thức gửi email
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Gửi email bất đồng bộ
        /// </summary>
        Task SendEmailAsync(string to, string subject, string body);
    }

    /// <summary>
    /// Dịch vụ gửi email trong hệ thống
    /// 
    /// Tính năng:
    /// - Gửi email xác thực tài khoản
    /// - Gửi email đặt lại mật khẩu
    /// - Gửi thông báo đơn hàng
    /// - Hỗ trợ HTML
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gửi email qua SMTP
        /// 
        /// Quy trình:
        /// 1. Kết nối SMTP
        /// 2. Tạo MailMessage
        /// 3. Gửi email
        /// 4. Xử lý lỗi
        /// </summary>
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient
                {
                    Host = _configuration["Email:Host"],
                    Port = int.Parse(_configuration["Email:Port"]),
                    EnableSsl = true,
                    Credentials = new NetworkCredential(
                        _configuration["Email:Username"],
                        _configuration["Email:Password"]
                    )
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_configuration["Email:From"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}
