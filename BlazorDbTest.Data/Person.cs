using System;

namespace BlazorDbTest.Data
{
    public class Person : DbObj<Person>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Birthdate { get; set; }
    }
}
