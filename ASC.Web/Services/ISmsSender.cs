﻿namespace ASC.Solution.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}