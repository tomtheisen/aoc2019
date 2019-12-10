<Query Kind="Statements" />

#load ".\helpers.linq"
#load ".\intcode.linq"

var machine = new IntCodeMachine();
machine.Outputting += n => Console.WriteLine(n);

machine.Reset();
machine.TakeInput(1);
machine.Run();

Console.WriteLine();

machine.Reset();
machine.TakeInput(5);
machine.Run();
