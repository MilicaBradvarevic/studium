using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain;

public class Student
{
    public string Ime { get; set; }
    public uint StudentId { get; set; }


    public Student() { Ime = ""; StudentId = 0; }
    public Student(string ime, uint id)
    {
        Ime = ime;
        StudentId = id;
    }

    public Student(Student student)
    {
        Ime = student.Ime;
        StudentId = student.StudentId;
    }
}
