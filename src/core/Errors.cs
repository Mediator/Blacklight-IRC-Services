using System.Diagnostics;
using System;
using System.Data;
using System.Collections;

namespace BlackLight
{
	namespace Services
	{
		namespace Error
		{
			
			/// -----------------------------------------------------------------------------
			/// Project	 : core
			/// Class	 : Services.Error.Errors
			///
			/// -----------------------------------------------------------------------------
			/// <summary>
			/// Used for error output in all files
			/// </summary>
			/// <remarks>
			/// </remarks>
			/// <history>
			/// 	[Caleb]	8/30/2006	Created
			/// </history>
			/// -----------------------------------------------------------------------------
			[FlagsAttribute]public enum Errors
			{
				DEBUG = 1,
				WARNING = 2,
				@ERROR = 4,
				FATAL = 8,
				ALL = DEBUG | WARNING | @ERROR | FATAL,
			}
		}
	}
	
	
}
