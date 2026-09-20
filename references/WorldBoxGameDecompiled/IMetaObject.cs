using System.Collections.Generic;
using UnityEngine;

public interface IMetaObject : ICoreObject
{
	int getMaxPossibleLifespan()
	{
		int num = 0;
		int num2 = 0;
		foreach (Actor unit in getUnits())
		{
			int num3 = (int)unit.stats["lifespan"];
			if (num3 > num2)
			{
				num2 = num3;
			}
			num++;
		}
		if (num == 0)
		{
			return 100;
		}
		return num2;
	}

	float getRatioAdults()
	{
		int num = 0;
		float num2 = 0f;
		foreach (Actor unit in getUnits())
		{
			if (unit.isAdult())
			{
				num2++;
			}
			num++;
		}
		if (num <= 0)
		{
			return 0f;
		}
		return num2 / (float)num;
	}

	float getRatioMales()
	{
		int num = 0;
		float num2 = 0f;
		foreach (Actor unit in getUnits())
		{
			if (unit.isSexMale())
			{
				num2++;
			}
			num++;
		}
		if (num <= 0)
		{
			return 0f;
		}
		return num2 / (float)num;
	}

	float getRatioFemales()
	{
		int num = 0;
		float num2 = 0f;
		foreach (Actor unit in getUnits())
		{
			if (unit.isSexFemale())
			{
				num2++;
			}
			num++;
		}
		if (num <= 0)
		{
			return 0f;
		}
		return num2 / (float)num;
	}

	float getRatioChildren()
	{
		int num = 0;
		float num2 = 0f;
		foreach (Actor unit in getUnits())
		{
			if (!unit.isAdult())
			{
				num2++;
			}
			num++;
		}
		if (num <= 0)
		{
			return 0f;
		}
		return num2 / (float)num;
	}

	float getRatioHoused()
	{
		int num = 0;
		float num2 = 0f;
		foreach (Actor unit in getUnits())
		{
			if (unit.hasHouse())
			{
				num2++;
			}
			num++;
		}
		if (num <= 0)
		{
			return 0f;
		}
		return num2 / (float)num;
	}

	float getRatioHomeless()
	{
		int num = 0;
		float num2 = 0f;
		foreach (Actor unit in getUnits())
		{
			if (!unit.hasHouse())
			{
				num2++;
			}
			num++;
		}
		if (num <= 0)
		{
			return 0f;
		}
		return num2 / (float)num;
	}

	float getRatioHungry()
	{
		int num = 0;
		float num2 = 0f;
		foreach (Actor unit in getUnits())
		{
			if (unit.isHungry())
			{
				num2++;
			}
			num++;
		}
		if (num <= 0)
		{
			return 0f;
		}
		return num2 / (float)num;
	}

	float getRatioStarving()
	{
		int num = 0;
		float num2 = 0f;
		foreach (Actor unit in getUnits())
		{
			if (unit.isStarving())
			{
				num2++;
			}
			num++;
		}
		if (num <= 0)
		{
			return 0f;
		}
		return num2 / (float)num;
	}

	float getRatioSick()
	{
		int num = 0;
		float num2 = 0f;
		foreach (Actor unit in getUnits())
		{
			if (unit.isSick())
			{
				num2++;
			}
			num++;
		}
		if (num <= 0)
		{
			return 0f;
		}
		return num2 / (float)num;
	}

	float getRatioHappy()
	{
		int num = 0;
		float num2 = 0f;
		foreach (Actor unit in getUnits())
		{
			if (unit.isHappy())
			{
				num2++;
			}
			num++;
		}
		if (num <= 0)
		{
			return 0f;
		}
		return num2 / (float)num;
	}

	float getRatioUnhappy()
	{
		int num = 0;
		float num2 = 0f;
		foreach (Actor unit in getUnits())
		{
			if (unit.isUnhappy())
			{
				num2++;
			}
			num++;
		}
		if (num <= 0)
		{
			return 0f;
		}
		return num2 / (float)num;
	}

	MetaTypeAsset getMetaTypeAsset();

	bool hasUnits();

	int countUnits();

	IEnumerable<Actor> getUnits();

	Actor getRandomUnit();

	Actor getRandomActorForReaper();

	int countFamilies();

	IEnumerable<Family> getFamilies();

	bool hasFamilies();

	ActorAsset getActorAsset();

	Sprite getSpriteIcon();

	bool isCursorOver();

	void setCursorOver();

	ColorAsset getColor();

	MetaObjectData getMetaData();

	int getRenown();

	int getPopulationPeople();

	long getTotalKills();

	long getTotalDeaths();

	bool isSelected();

	Actor getOldestVisibleUnit();

	Actor getOldestVisibleUnitForNameplatesCached();

	bool hasCities();

	IEnumerable<City> getCities();

	bool hasKingdoms();

	IEnumerable<Kingdom> getKingdoms();

	bool hasDied();
}
