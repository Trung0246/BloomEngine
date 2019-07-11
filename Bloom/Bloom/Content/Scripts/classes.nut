class CircleEmitter extends BulletEmitter
{
	constructor(position, angle)
	{
	    base.constructor(position, angle);
	}

	function Fire()
	{
		const COUNT = 50;

		local bulletRegion = TextureRegion(GetContent("TestBullets"), Vector(1, 1), Vector(16, 16));
		for (local i = 0; i < COUNT; i++)
		{
		    FireBullet(
		    		Position,
		    		Angle + (i / (COUNT - 1.0)) * 360,
		    		30,
		    		bulletRegion
		    	);
		}
	}
}

function TestEmitter()
{
	local circleTest = CircleEmitter(Vector(0, 0), 0);
	local timer = Timer(0.1, -1, function () { circleTest.Angle += 1; circleTest.Fire(); });
}