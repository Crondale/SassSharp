# SassSharp
Managed implementation of Sass in C#.  Only SCSS is supported for now.

## How to use
SassSharp is in development and should not be used for live projects, but please clone it and play around with it.

```
var compiler = new ScssCompiler();
var result = compiler.CompileFile(sourcePath);
string cssContent = sres.CompiledContent;
```

##Roadmap feature support
* ~~Nested Selectors~~
* ~~Lists~~
* ~~Sass script~~
* ~~Comments~~
* ~~Interpolation~~
* ~~Nested properties~~
* ~~Variables~~

* ~~@import~~
* ~~@mixin~~
* ~~@media~~
* ~~@if~~
* ~~@each~~
* @for

##Contribution
New features and bug fixes is done tests first.  Tests are done by creating scss files in testfiles folder and are automatically compared using LibSass.

