using System.Collections.Generic;

namespace Contacts.API.ViewModel
{
    public class TagContactInputViewModel
    {
        public int ContactId { get; set; }
        public List<string> Tags { get; set; }
    }
}