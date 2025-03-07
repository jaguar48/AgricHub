using System;

namespace AgricHub.BLL.Interfaces.Email;

public interface IEmail
{
     void Send(string to, string subject, string html, string? from = null);
}
