using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Course.Classes
{
    public class GoogleApi
    {
        private readonly string ApplicationName = "Notes by Broniev";
        private readonly string CalendarId = "primary";
        private UserCredential credential;
      
        public async Task LogToAccount()
        {
            try
            {
                string clientId = "Put your client id here";
                string clientSecret = "Put your client secret here";
                string[] scopes = { CalendarService.Scope.Calendar, "https://www.googleapis.com/auth/userinfo.profile", GmailService.Scope.GmailReadonly };

                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret,
                    },
                    scopes,
                    "user",
                    CancellationToken.None
                );

                if (credential.Token.IsExpired(SystemClock.Default))
                {
                    await credential.RefreshTokenAsync(CancellationToken.None);
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        public async Task LeaveAccountAsync()
        {
            await LogToAccount();
            var cancellationTokenSource = new CancellationTokenSource();
            await credential.RevokeTokenAsync(cancellationTokenSource.Token);
        }

        public string GetCurrentEmail()
        {
            var gmailService = GetGmailService();
            var gmailProfile = gmailService.Users.GetProfile("me").Execute();
            var userGmailEmail = gmailProfile.EmailAddress;
            return userGmailEmail;
        }

        private CalendarService GetCalendarService()
        {
            return new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        private GmailService GetGmailService()
        {
            return new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
            });
        }

        public async Task AddEvent(DateTime startDate, DateTime endDate, Note note)
        {
            await LogToAccount();
                var service = GetCalendarService();

                var newEvent = new Event
                {
                    Summary = note.Name,
                    Start = new EventDateTime { DateTime = startDate },
                    End = new EventDateTime { DateTime = endDate },
                    Description = note.PlainText,

                    Reminders = new Event.RemindersData()
                    {
                        UseDefault = false,
                        Overrides = new EventReminder[] {
                    new EventReminder() { Method = "email", Minutes = 10 },
                    new EventReminder() { Method = "popup", Minutes = 1 },
                    }
                    }
                };

                string calendarId = "primary";
                EventsResource.InsertRequest request = service.Events.Insert(newEvent, calendarId);
                Event createdEvent = request.Execute();
                note.EvId = createdEvent.Id;
                note.URL = createdEvent.HtmlLink;
        }

        public async Task DeleteEvent(string eventId)
        {
            await LogToAccount();
            var service = GetCalendarService();
 
            try
            {
                service.Events.Delete(CalendarId, eventId).Execute();
                ToastNotifications.PopToast("Deleted", "Remind successfully deleted");
            }
            catch (Exception)
            {
                ToastNotifications.PopToast("Erorr", "Remind deleted or doesn't exsist");
            }
        }
    }
}
