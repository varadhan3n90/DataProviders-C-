// <copyright file="GlobalSuppressions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1309:Field names should not begin with underscore", Justification = "For users of resharper")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "Using underscore instead of this prefix")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "If value not present in cache or not able to contact cache, program should not fail", Scope = "member", Target = "~M:DataAccessService.RedisCache.RedisCacheProvider.GetData``1(System.Object,System.Action{``0,System.Object})~System.Threading.Tasks.Task{``0}")]
[assembly: SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed", Scope = "member", Target = "~M:DataAccessService.SQL.SQLDataProvider.GetDbReader(DataAccessService.SQL.SQLQueryOptions)~System.Threading.Tasks.Task{System.Object}")]
