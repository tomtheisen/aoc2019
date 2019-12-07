<Query Kind="Statements" />

#load ".\helpers.linq"
int min = (int)GetAocUIntegers()[0];
int max = (int)GetAocUIntegers()[1];

int result1 = 0, result2 = 0;
for (int n = min; n <= max; n++) {
	string s = n.ToString();
	if (
		s[0] <= s[1] &&
		s[1] <= s[2] &&
		s[2] <= s[3] &&
		s[3] <= s[4] &&
		s[4] <= s[5] && (
			s[0] == s[1] ||
			s[1] == s[2] ||
			s[2] == s[3] ||
			s[3] == s[4] ||
			s[4] == s[5]))
	{
		result1 += 1;
		if (
							s[0] == s[1] && s[1] != s[2] ||
			s[0] != s[1] && s[1] == s[2] && s[2] != s[3] ||
			s[1] != s[2] && s[2] == s[3] && s[3] != s[4] ||
			s[2] != s[3] && s[3] == s[4] && s[4] != s[5] ||
			s[3] != s[4] && s[4] == s[5]
		) result2 += 1;
	}
}

Console.WriteLine(result1);
Console.WriteLine(result2);