﻿using System;
using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.OS;
using AndroidX.AppCompat.App;
using Com.Telerik.Widget.Calendar;
using Com.Telerik.Widget.Calendar.Events;
using Java.Util;

namespace App1
{
    [Activity(Label = "@string/action_calendar", Theme = "@style/AppTheme")]
    public class CalendarActivity : AppCompatActivity
    {
        CosmosAccess cosmosAccess = new CosmosAccess();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_calendar);

            
            var temp = cosmosAccess.RetrieveDict(); 

            RadCalendarView calendarView = (RadCalendarView)FindViewById(Resource.Id.calendarView);
            calendarView.SelectionMode = CalendarSelectionMode.Single;

            Calendar calendar = Java.Util.Calendar.Instance;
            Date date;
            long start = calendar.TimeInMillis;
            int year = SettingsHelper.Year;
            int month = SettingsHelper.Month;
            int day = SettingsHelper.Day;
            string snippetTime = String.Format("{0}-{1}", year.ToString(), month.ToString());
            calendar.Add(CalendarField.Hour, 3);
            long end = calendar.TimeInMillis;
            //Event newEvent = new Event(temp["sample1"].ToString(), start, end);
            ////temp.["sample1"] is the key in the collection 

            //calendar.Add(CalendarField.Hour, 1);
            //start = calendar.TimeInMillis;
            //calendar.Add(CalendarField.Hour, 1);
            //end = calendar.TimeInMillis;
            //Event newEvent2 = new Event("Walk to work", start, end);
            //newEvent2.EventColor = Android.Graphics.Color.Green;

            //calendar.Add(CalendarField.Hour, 1);

            string answer ="";
            IList<Event> events = new List<Event>();
            int counter = 0;
            float totalSpend = 0;
            double tempSaving = 0;
            //int tempCounter = 0;

            foreach (List<string> lister in cosmosAccess.CashList)
            {
                int[] dates = new int[3];
                if (lister.Count != 0)
                {
                    int seCount = 0;
                    float tempValue = 0;
                    float currBill = 0;
                    Event nEvent;
                    Event sEvent;
                    bool printSum = false;
                    foreach (string stringList in lister)
                    {
                        if (seCount == 0)
                        {
                            string[] time = stringList.Split("T");
                            if (time.Length == 2)
                            {
                                string[] dateTimes = time[0].Split("-");
                                string[] clockTimes = time[1].Split(":");
                                int timeCheck = Int32.Parse(clockTimes[0]) - 5;
                                dates[0] = Int32.Parse(dateTimes[0]);
                                dates[1] = Int32.Parse(dateTimes[1]) - 1;
                                dates[2] = Int32.Parse(dateTimes[2]);

                                if (dates[2] == 1 && timeCheck < 0)
                                {
                                    dates[1] -= 1;

                                    if (dates[1] == 1)
                                    {
                                        dates[2] = 28;
                                    }
                                    else if(dates[1] == 0 || dates[1] == 2 || dates[1] == 4 || dates[1] == 6 || dates[1] == 7 || dates[1] == 9 || dates[1] == 11)
                                    {
                                        dates[2] = 31; 
                                    }
                                    else
                                    {
                                        dates[2] = 30;
                                    }
                                }
                                else if(timeCheck < 0)
                                {
                                    dates[2] -= 1;
                                }

                                date = new Date(Int32.Parse(dateTimes[0])-1900, (dates[1]), dates[2]);
                                calendar.Time = date;//  .setTime(date);
                                seCount++;
                            }
                            else
                            {
                                seCount++;
                            }
                        }

                        if (SettingsHelper.Day == 0)
                        {
                            if(dates[0] == SettingsHelper.Year && ((dates[1]+1) == SettingsHelper.Month))
                            {
                                try
                                {
                                    tempValue = float.Parse(stringList, System.Globalization.CultureInfo.InvariantCulture);
                                    if (currBill <= tempValue)
                                    {
                                        currBill = tempValue;
                                        answer = stringList;
                                        printSum = true;
                                    }
                                }
                                catch (Exception e)
                                {
                                   
                                }

                                /*start = calendar.TimeInMillis;
                                calendar.Add(CalendarField.Minute, 1);
                                answer = stringList;
                                end = calendar.TimeInMillis;
                                nEvent = new Event(answer, start, end);
                                nEvent.EventColor = Android.Graphics.Color.Red;
                                events.Add(nEvent);*/
                            }
                            else
                            {
                                printSum = false;
                            }

                        }
                        else
                        {
                            if (dates[0] == SettingsHelper.Year && ((dates[1] + 1) == SettingsHelper.Month) && dates[2] == SettingsHelper.Day)
                            {
                                try
                                {
                                    tempValue = float.Parse(stringList, System.Globalization.CultureInfo.InvariantCulture);
                                    if (currBill <= tempValue)
                                    {
                                        currBill = tempValue;
                                        answer = stringList;
                                        printSum = true;
                                    }
                                }
                                catch(Exception e)
                                {
                                    
                                }


                                /*start = calendar.TimeInMillis;
                                calendar.Add(CalendarField.Minute, 1);
                                answer = stringList;
                                end = calendar.TimeInMillis;
                                nEvent = new Event(answer, start, end);
                                nEvent.EventColor = Android.Graphics.Color.Red;
                                events.Add(nEvent);*/
                            }
                            else
                            {
                                printSum = false;
                            }
                        }

                    }
                    if (printSum)
                    {
                        start = calendar.TimeInMillis;
                        calendar.Add(CalendarField.Minute, 1);
                        end = calendar.TimeInMillis;
                        SavingHelper.Cost = currBill;
                        SavingHelper.ConvertBills(currBill.ToString());
                        double saving = SavingHelper.Cost - currBill;
                        saving = Math.Round(saving, 2);
                        saving.ToString("0.00");
                        nEvent = new Event(String.Format("Bill {0} : {1}$", ++counter, answer), start, end);
                        sEvent = new Event(String.Format("Bill {0} Saving : {1}$ ", counter, saving), start, end);
                        nEvent.EventColor = Android.Graphics.Color.Blue;
                        sEvent.EventColor = Android.Graphics.Color.Red;
                        events.Add(nEvent);
                        events.Add(sEvent);
                        tempSaving += SavingHelper.Cost;
                        totalSpend += currBill;

                    }
                }
              
            }
            
            counter = 0;
            string tempMonth = SettingsHelper.Month.ToString();
            string tempYear = SettingsHelper.Year.ToString();
            SettingsHelper.ConvertVals("31", tempMonth, tempYear);
            if (SettingsHelper.Day == 0)
            {
                SettingsHelper.ConvertVals("30", tempMonth, tempYear);
                if(SettingsHelper.Day == 0)
                {
                    SettingsHelper.ConvertVals("28", tempMonth, tempYear);
                }
            }

            
            calendar.Time = new Date(SettingsHelper.Year - 1900, (SettingsHelper.Month - 1), SettingsHelper.Day);
            start = calendar.TimeInMillis; 
            end = calendar.TimeInMillis;
            calendar.Add(CalendarField.Hour, 7);
            double tempTotal = tempSaving - totalSpend;
            tempTotal = Math.Round(tempTotal, 2);
            Event totalEvent = new Event(String.Format("Total Bills: {0}$ ", totalSpend), start, end);
            Event tempEvent = new Event(String.Format("Total Savings : {0}$ ", tempTotal), start, end);
            totalEvent.EventColor = Android.Graphics.Color.ParseColor("#00375D");
            tempEvent.EventColor = Android.Graphics.Color.ParseColor("#008744");
            events.Add(totalEvent);
            events.Add(tempEvent);
            
            //events.Add(newEvent);
            //events.Add(newEvent2);

            calendarView.EventAdapter.Events = events;
            calendarView.EventsDisplayMode = EventsDisplayMode.Inline;
            calendarView.Adapter.InlineEventTimeStartTextColor = Color.ParseColor("#eeeeee");
            calendarView.Adapter.InlineEventTimeEndTextColor = Color.ParseColor("#eeeeee");
            calendarView.Adapter.InlineEventsBackgroundColor = Color.ParseColor("#eeeeee");

            // Create your application here
        }
    }
}