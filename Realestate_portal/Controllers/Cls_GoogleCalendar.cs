using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace Realestate_portal.Controllers
{
    public class Cls_GoogleCalendar
    {
        public void Test_googleEvents() {
            string jsonFile = HttpContext.Current.Server.MapPath("~/Content/poetic-producer-280700-821c738ea972.json");
            string calendarId = @"58p0dlp7hvhgloss62e8jv341k@group.calendar.google.com";

            string[] Scopes = { CalendarService.Scope.Calendar };

            ServiceAccountCredential credential;

            using (var stream =
                new FileStream(jsonFile, FileMode.Open, FileAccess.Read))
            {
                var confg = Google.Apis.Json.NewtonsoftJsonSerializer.Instance.Deserialize<JsonCredentialParameters>(stream);
                credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(confg.ClientEmail)
                   {
                       Scopes = Scopes
                   }.FromPrivateKey(confg.PrivateKey));
            }

            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Calendar API Sample",
            });

            var calendar = service.Calendars.Get(calendarId).Execute();
            Debug.WriteLine("Calendar Name :");
            Debug.WriteLine(calendar.Summary);

   


            // Define parameters of request.
            EventsResource.ListRequest listRequest = service.Events.List(calendarId);
            listRequest.TimeMin = DateTime.Now;
            listRequest.ShowDeleted = false;
            listRequest.SingleEvents = true;
            listRequest.MaxResults = 10;
            listRequest.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = listRequest.Execute();
            Debug.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    Debug.WriteLine("{0} ({1})", eventItem.Summary, when);
                }
            }
            else
            {
                Debug.WriteLine("No upcoming events found.");
            }

            //Insert event
            Event createdEvent = null;
            Event myEvent = new Event
            {
                Summary = "Test appointment from app PGR",
                Description = "Inside description for event",
                Location = "Here",
                Start = new EventDateTime()
                {
                    DateTime = new DateTime(2020,6, 26, 9, 0, 0),
                    TimeZone = "America/New_York"
                },
                End = new EventDateTime()
                {
                    DateTime = new DateTime(2020, 6, 26, 10, 0, 0),
                    TimeZone = "America/New_York"
                },
                Recurrence = new String[]
        {
                //"RRULE:FREQ=DAILY;COUNT=2"
        }
            };


            Debug.WriteLine("calling service.Events.Insert( newEvent, calendarId )");
            EventsResource.InsertRequest request = service.Events.Insert(myEvent, calendarId);
            createdEvent = request.Execute();
            Debug.WriteLine("Event created: {0}", createdEvent.HtmlLink);

            /////////
            //var myevent = DB.Find(x => x.Id == "eventid" + 1);

            //var InsertRequest = service.Events.Insert(myevent, calendarId);

            //try
            //{
            //    InsertRequest.Execute();
            //}
            //catch (Exception ex)
            //{
            //    try
            //    {
            //        service.Events.Update(myevent, calendarId, myevent.Id).Execute();
            //        Console.WriteLine("Insert/Update new Event ");
            //        Console.Read();

            //    }
            //    catch (Exception)
            //    {
            //        Console.WriteLine("can't Insert/Update new Event ");

            //    }
            //}
        }

        public static DateTime GetNYtime(DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime,
                TimeZoneInfo.FindSystemTimeZoneById("US Eastern Standard Time"));
        }

        public List<DateTime> get_events(DateTime fecha)
        {
            List<DateTime> list = new List<DateTime>();

            list.Add(new DateTime(fecha.Year, fecha.Month, fecha.Day, 9, 0, 0));
            list.Add(new DateTime(fecha.Year, fecha.Month, fecha.Day, 10, 0, 0));
            list.Add(new DateTime(fecha.Year, fecha.Month, fecha.Day, 11, 0, 0));
            list.Add(new DateTime(fecha.Year, fecha.Month, fecha.Day, 12, 0, 0));
            list.Add(new DateTime(fecha.Year, fecha.Month, fecha.Day, 13, 0, 0));
            list.Add(new DateTime(fecha.Year, fecha.Month, fecha.Day, 14, 0, 0));
            list.Add(new DateTime(fecha.Year, fecha.Month, fecha.Day, 15, 0, 0));
            list.Add(new DateTime(fecha.Year, fecha.Month, fecha.Day, 16, 0, 0));
            list.Add(new DateTime(fecha.Year, fecha.Month, fecha.Day, 17, 0, 0));
            list.Add(new DateTime(fecha.Year, fecha.Month, fecha.Day, 18, 0, 0));
            list.Add(new DateTime(fecha.Year, fecha.Month, fecha.Day, 19, 0, 0));
  



            string jsonFile = HttpContext.Current.Server.MapPath("~/Content/poetic-producer-280700-821c738ea972.json");
            string calendarId = @"58p0dlp7hvhgloss62e8jv341k@group.calendar.google.com";

            string[] Scopes = { CalendarService.Scope.Calendar };

            ServiceAccountCredential credential;

            using (var stream =
                new FileStream(jsonFile, FileMode.Open, FileAccess.Read))
            {
                var confg = Google.Apis.Json.NewtonsoftJsonSerializer.Instance.Deserialize<JsonCredentialParameters>(stream);
                credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(confg.ClientEmail)
                   {
                       Scopes = Scopes
                   }.FromPrivateKey(confg.PrivateKey));
            }

            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Calendar API Sample",
            });

            var calendar = service.Calendars.Get(calendarId).Execute();

            // Define parameters of request.
            EventsResource.ListRequest listRequest = service.Events.List(calendarId);
            listRequest.TimeMin = new DateTime(fecha.Year, fecha.Month,fecha.Day, 9,0,0);
            listRequest.TimeMax = new DateTime(fecha.Year, fecha.Month, fecha.Day, 20, 0, 0);
            listRequest.ShowDeleted = false;
            listRequest.SingleEvents = true;
            listRequest.MaxResults = 18;
            listRequest.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = listRequest.Execute();
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    var eventdate = Convert.ToDateTime(eventItem.Start.DateTime);
                    var exist = (from a in list where (a == eventdate) select a).Count();
                    if (exist > 0) { list.Remove(eventdate); }
                    else {
                        //Para El Salvador son 2 horas de diferencia
                        //list.Add(eventdate);
                    }
               

                    //string when = eventItem.Start.DateTime.ToString();
                    //if (String.IsNullOrEmpty(when))
                    //{
                    //    when = eventItem.Start.Date;
                    //}
                    //Debug.WriteLine("{0} ({1})", eventItem.Summary, when);
                }
            }

            return list;
        }

        public void POST_googleEvents(DateTime fecha, DateTime time, string title, string details)
        {
            string jsonFile = HttpContext.Current.Server.MapPath("~/Content/poetic-producer-280700-821c738ea972.json");
            string calendarId = @"58p0dlp7hvhgloss62e8jv341k@group.calendar.google.com";

            string[] Scopes = { CalendarService.Scope.Calendar };

            ServiceAccountCredential credential;

            using (var stream =
                new FileStream(jsonFile, FileMode.Open, FileAccess.Read))
            {
                var confg = Google.Apis.Json.NewtonsoftJsonSerializer.Instance.Deserialize<JsonCredentialParameters>(stream);
                credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(confg.ClientEmail)
                   {
                       Scopes = Scopes
                   }.FromPrivateKey(confg.PrivateKey));
            }

            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Calendar API Sample",
            });

            var calendar = service.Calendars.Get(calendarId).Execute();
 
            //Insert event
            Event createdEvent = null;
            Event myEvent = new Event
            {
                Summary = title,
                Description = details,
                Start = new EventDateTime() //-2 por la zona horaria el salvador
                {
                    DateTime = new DateTime(fecha.Year, fecha.Month, fecha.Day, time.Hour, 0, 0),
                    TimeZone = "America/New_York"
                },
                End = new EventDateTime()
                {
                    DateTime = new DateTime(fecha.Year, fecha.Month, fecha.Day, time.AddHours(1).Hour, 0, 0),
                    TimeZone = "America/New_York"
                },
                Recurrence = new String[]
        {
                //"RRULE:FREQ=DAILY;COUNT=2"
        }
            };


            Debug.WriteLine("calling service.Events.Insert( newEvent, calendarId )");
            EventsResource.InsertRequest request = service.Events.Insert(myEvent, calendarId);
            createdEvent = request.Execute();
            Debug.WriteLine("Event created: {0}", createdEvent.HtmlLink);


        }

    }
}