class TestEnemy extends Enemy
{
	Name = "Unnamed";

	constructor(name)
	{
		Name = name;
	}

	function PrintName()
	{
		Console.WriteLine("My name is " + Name + "!");
	}
}

function main()
{
	Console.WriteLine("Hello!");
	TestEnemyInstance <- TestEnemy("Foo");
}