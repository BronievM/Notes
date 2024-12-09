using System;
using System.Collections.Generic;

namespace Course.Classes
{
    public class NoteBuilder
    {
        private string Name;
        private string RTF;
        private string EvId;
        private string URL;
        private DateTime time;
        private DateTime? timeOfNot;
        private Account account;
        private List<string> tags;
        private bool isPrivate;
        private bool isEventCr;
        private bool isHidden;
        private bool isArchived;

        public NoteBuilder()
        {
            Name = String.Empty;
            RTF = String.Empty;
            EvId = String.Empty;
            URL = String.Empty;
            time = DateTime.MinValue;
            timeOfNot = DateTime.MinValue;
            account = new Account();
            tags = new List<string>();
            isPrivate = false;
            isEventCr = false;
            isHidden = false;
            isArchived = false;
        }

        public NoteBuilder WithName(string name)
        {
            Name = name;
            return this;
        }

        public NoteBuilder WithRTF(string rtf)
        {
            RTF = rtf;
            return this;
        }

        public NoteBuilder WithEvId(string evId)
        {
            EvId = evId;
            return this;
        }

        public NoteBuilder WithURL(string url)
        {
            URL = url;
            return this;
        }

        public NoteBuilder WithTime(DateTime time)
        {
            this.time = time;
            return this;
        }

        public NoteBuilder WithTimeOfNot(DateTime? timeOfNot)
        {
            this.timeOfNot = timeOfNot;
            return this;
        }

        public NoteBuilder WithAccount(Account account)
        {
            this.account = account;
            return this;
        }

        public NoteBuilder WithTags(List<string> tags)
        {
            this.tags = tags;
            return this;
        }

        public NoteBuilder WithIsPrivate(bool isPrivate)
        {
            this.isPrivate = isPrivate;
            return this;
        }

        public NoteBuilder WithIsEventCr(bool isEventCr)
        {
            this.isEventCr = isEventCr;
            return this;
        }

        public NoteBuilder WithIsHidden(bool isHidden)
        {
            this.isHidden = isHidden;
            return this;
        }

        public NoteBuilder WithIsArchived(bool isArchived)
        {
            this.isArchived = isArchived;
            return this;
        }

        public Note Build()
        {
            return new Note
            {
                Name = Name,
                RTF = RTF,
                EvId = EvId,
                URL = URL,
                time = time,
                timeOfNot = timeOfNot,
                AccountN = account,
                Tags = tags,
                IsPrivate = isPrivate,
                IsEventCr = isEventCr,
                IsHidden = isHidden,
                IsArchived = isArchived
            };
        }
    }
}
