<Query Kind="Statements" />

#load ".\helpers.linq"

// part 1
GetAocIntegers().Sum(n => n / 3 - 2).Dump();

// part 2
int AllFuel(int n) {
	int result = 0;
	while (n > 8) result += n = n / 3 - 2;
	return result;
}
GetAocIntegers().Sum(AllFuel).Dump();
