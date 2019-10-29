﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using OrleansDashboard;
using OrleansDashboard.Model;

namespace OrleansDashboard
{
    public class DashboardRemindersGrain : Grain, IDashboardRemindersGrain
    {
        private static readonly Immutable<ReminderResponse> EmptyReminders = new ReminderResponse
        {
            Reminders = new ReminderInfo[0]
        }.AsImmutable();

        private readonly IReminderTable _reminderTable;

        public DashboardRemindersGrain(IServiceProvider serviceProvider)
        {
            _reminderTable = serviceProvider.GetService(typeof(IReminderTable)) as IReminderTable;
        }

        public async Task<Immutable<ReminderResponse>> GetReminders(int pageNumber, int pageSize)
        {
            if (_reminderTable == null)
            {
                return EmptyReminders;
            }

            var reminderData = await _reminderTable.ReadRows(0, 0xffffffff);

            if(!reminderData.Reminders.Any())
            {
                return EmptyReminders;
            }

            return new ReminderResponse
            {
                Reminders = reminderData
                    .Reminders
                    .OrderBy(x => x.StartAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ToReminderInfo)
                    .ToArray(),

                Count = reminderData.Reminders.Count
            }.AsImmutable();
        }

        private static ReminderInfo ToReminderInfo(ReminderEntry entry)
        {
            return new ReminderInfo
            {
                PrimaryKey = entry.GrainRef.PrimaryKeyAsString(),
                GrainReference = entry.GrainRef.ToString(),
                Name = entry.ReminderName,
                StartAt = entry.StartAt,
                Period = entry.Period,
            };
        }
    }
}