function main()
{
	local enemy = Enemy(SpriteImage(GetContent("TestEnemy"), Vector(0, 0), Vector(64, 64)), 100);

	local emitter1 = Emitter();
	emitter1.Animation = Animation(
			[
				Instruction.SetScale(0, Vector(64, 64)),
				Instruction.LerpScale(0, 0.2, Vector(16, 16))
				Instruction.Stop()
			]
		);
	emitter1.Image = SpriteImage(GetContent("TestBullets"), Vector(1, 1), Vector(16, 16));
	emitter1.Angle2 = 70;
	emitter1.Count1 = 37;
	emitter1.Count2 = 80;
	emitter1.Speed1 = 700;
	emitter1.Speed2 = 150;

	local emitter2 = Emitter();
	emitter2.Animation = Animation(
			[
				Instruction.SetScale(0, Vector(64, 64)),
				Instruction.LerpScale(0, 0.2, Vector(16, 16))
				Instruction.Stop()
			]
		);
	emitter2.Image = SpriteImage(GetContent("TestBullets"), Vector(19, 1), Vector(16, 16));
	emitter2.Angle2 = 70;
	emitter2.Count1 = 37;
	emitter2.Count2 = 80;
	emitter2.Speed1 = 700;
	emitter2.Speed2 = 150;

	local ang = 0;
	Timer(
			0.05, -1,
			function()
			{
				print(enemy.Velocity);
				enemy.Velocity = Vector(cos(ang) * 100, sin(ang) * 100, 0);
			}
		);
	Timer(
			0.2, -1,
			function()
			{
				emitter1.Fire(Vector(enemy.Position.X, enemy.Position.Y), ang);
				ang += 25;
			}
		);
	Timer(
			0.3, -1,
			function()
			{
				emitter2.Fire(Vector(enemy.Position.X, enemy.Position.Y), ang + 180);
				ang += 25 + 25 / 2;
			}
		);
}