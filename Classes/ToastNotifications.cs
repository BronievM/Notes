using System;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Course.Classes
{
    internal class ToastNotifications
    {
        public static void PopToast(string title, string text)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(text)
                .Show((toast =>
                {
                    toast.ExpirationTime = DateTime.Now.AddHours(1);
                }));
        }

        public static void PopToast(string title, string text, string ButtonContent, string patch)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(text)
                .AddButton(new ToastButton(ButtonContent, $"patch={patch}")
                {
                    ActivationType = ToastActivationType.Foreground
                })
                .Show((toast =>
                {
                    toast.ExpirationTime = DateTime.Now.AddHours(1);
                }));
        }
    }
}
