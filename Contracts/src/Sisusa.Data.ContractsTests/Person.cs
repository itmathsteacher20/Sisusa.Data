using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisusa.Data.ContractsTests
{
    public enum Gender : ushort
    {
        Unknown = 0,

        Female = 1,

        Male = 2,
    }

    public class Person
    {
        public int Id { get; init; }

        public string FirstName { get; init; } = string.Empty;

        public string LastName { get; init; } = string.Empty;

        public DateTime DateOfBirth { get; init; }

        public Gender Gender { get; init; } = Gender.Unknown;
    }
}
