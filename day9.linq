<Query Kind="Statements">
  <Namespace>System.Numerics</Namespace>
</Query>

#load ".\helpers.linq"
#load ".\intcode.linq"

var machine = new IntCodeMachine();
machine.Outputting += n => Console.WriteLine(n);
machine.TakeInput(1);
machine.Run();

machine.Reset();
machine.TakeInput(2);
machine.Run();