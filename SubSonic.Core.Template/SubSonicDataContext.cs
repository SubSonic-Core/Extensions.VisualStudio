﻿using Microsoft.Extensions.DependencyInjection;
using SubSonic;
using System;

namespace $rootnamespace$
{
	public partial class $safeitemrootname$
		: SubSonicContext
	{
		private readonly IServiceCollection services = null;

		public $safeitemrootname$(IServiceCollection services)
		{
			this.services = services ?? throw new ArgumentNullException(nameof(services));
		}

		#region ISubSonicSetCollection{TEntity} Collection Properties
		#endregion
	}
}