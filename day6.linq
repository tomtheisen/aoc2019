<Query Kind="Statements" />

#load ".\helpers.linq"

var orbits = GetAocLines()
	.ToDictionary(l => l.Split(')')[1], l => l.Split(')')[0]);

List<string> GetParents(string obj) {
	var result = new List<string>();
	for (var cur = obj; cur != "COM"; cur = orbits[cur]) result.Add(cur);
	result.Add("COM");
	return result;
}
Console.WriteLine(orbits.Keys.Sum(obj => GetParents(obj).Count - 1));

List<string> you = GetParents("YOU"), san = GetParents("SAN");
int common = 1;
for (; you[^common] == san[^common]; common++) ;
Console.WriteLine(you.Count + san.Count - common * 2);