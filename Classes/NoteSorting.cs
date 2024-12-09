using System.Collections.Generic;

namespace Course.Classes
{
    public class NotesSorting
    {
        private List<Note> notes;

        public NotesSorting(List<Note> notes)
        {
            this.notes = notes;
        }

        public List<Note> SortNotesByAuthor(bool ascending = true)
        {
            return SortNotes("Author", ascending);
        }

        public List<Note> SortNotesByDate(bool ascending = true)
        {
            return SortNotes("Date", ascending);
        }

        public List<Note> SortNotesByName(bool ascending = true)
        {
            return SortNotes("Name", ascending);
        }

        public List<Note> SortNotesByText(bool ascending = true)
        {
            return SortNotes("Text", ascending);
        }

        private List<Note> SortNotes(string sortBy, bool ascending)
        {
            for (int i = 0; i < notes.Count - 1; i++)
            {
                for (int j = i + 1; j < notes.Count; j++)
                {
                    int comparisonResult = 0;
                    switch (sortBy)
                    {
                        case "Author":
                            comparisonResult = string.Compare(notes[i].AccountN.Name, notes[j].AccountN.Name);
                            break;
                        case "Date":
                            comparisonResult = notes[i].time.CompareTo(notes[j].time);
                            break;
                        case "Name":
                            comparisonResult = string.Compare(notes[i].Name, notes[j].Name);
                            break;
                        case "Text":
                            comparisonResult = string.Compare(notes[i].PlainText, notes[j].PlainText);
                            break;
                    }

                    if (ascending ? comparisonResult > 0 : comparisonResult < 0)
                    {
                        Swap(i, j);
                    }
                }
            }
            return notes;
        }

        private void Swap(int index1, int index2)
        {
            Note temp = notes[index1];
            notes[index1] = notes[index2];
            notes[index2] = temp;
        }
    }
}
