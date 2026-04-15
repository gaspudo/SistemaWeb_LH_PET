using System;
using System.Net;
using System.Net.Mail;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LH_PET_WEB.services
{
    public interface IEmailService
    {
        Task<bool> EnviarEmailAsync(string destinatario, string assunto, string mensagem);
    }

    public class EmailService : IEmailService 
    {
        private readonly IConfiguration _configuracao;

        public EmailService(IConfiguration configuracao)
        {
            _configuracao = configuracao;
        }

        public async Task<bool> EnviarEmailAsync(string destinatario, string assunto, string mensagem)
        {
            try
            {
                string servidor = _configuracao["SmtpConfig:Servidor"] ?? "smtp.office365.com";
                int porta = int.Parse(_configuracao["SmtpConfig:Porta"]) ?? 587;
                string remetente = _configuracao["SmtpConfig:Usuario"] ?? "";
                string senha = _configuracao["SmtpConfig:Senha"] ?? "";

                if (string.IsNullOrEmpty(remetente) || string.IsNullOrEmpty(senha))
                {
                    Console.WriteLine("AVISO: Configurações de email não estão completas.");
                    return false;
                }

                using (var correio = new MailMessage())
                {
                    correio.From = new MailAddress(remetente, "Sistema VETPlus Care");
                    correio.To.Add(destinatario);
                    correio.Subject = assunto;
                    correio.Body = mensagem;

                    correio.IsBodyHtml = true;

                    using (var clienteSmtp = new SmtpClient(servidor, porta))
                    {
                        clienteSmtp.Credentials = new NetworkCredential(remetente, senha);
                        clienteSmtp.EnableSsl = true;
                        clienteSmtp.UseDefaultCredentials = false;

                        await clienteSmtp.SendMailAsync(correio);

                        Console.WriteLine($"Email enviado para {destinatario} com sucesso.");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log do erro pode ser adicionado aqui
                Console.WriteLine(NewString('=', 30));
                Console.WriteLine($"Erro ao enviar email para {destinatario}");
                Console.WriteLine($"Detalhes do erro: {ex.Message}");
               Console.WriteLine(NewString('=', 30));

                return false;
            }
        }
    }
}