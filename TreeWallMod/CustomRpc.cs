using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeWallMod
{
	public enum TreeWallModRpcsEnum : uint
	{
		ChangeAnimation,
		CosmeticControl,

		RunnerUpdateMoving,
		SetRunnerSpeed,
        AddPlayerSyringeInject,
        RemovePlayerSyringeInject,
        MarksmanSuppressedComplete,
		MarksmanWarp,
		WEEEStrech
    }
}
