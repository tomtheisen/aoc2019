<Query Kind="Statements" />

#load ".\helpers.linq"
#load ".\intcode.linq"

var machine = new IntCodeMachine(GetAocIntegers());
machine.Outputting += Console.WriteLine;

machine.Reset();
machine.TakeInput(1);
machine.Run();

Console.WriteLine();

machine.Reset();
machine.TakeInput(5);
machine.Run();