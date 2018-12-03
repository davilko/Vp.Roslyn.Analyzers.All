# ("Roslyn") Analyzers
This repository contains a number of Roslyn static analyzers, to help you write code more safely.

Projects
========

Vp.ConfigureAwait.Analyzer
--------------------------------
ConfigureAwait analyzer will remind you to use configureAwait in async code 


Vp.DateTimeNow.Analyzer
--------------------------------
DateTimeNow analyzer will offer to you use DateTime.UtcNow rather than DateTime.Now

Build via [C# Cake](https://github.com/cake-build/cake) 
========

Getting Started
===============

1. Clone the repository
2. Restore, Build, Run Tests: `build.ps1`

