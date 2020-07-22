# Build CowSpeak as a library
csc -target:library -out:CowSpeak.dll /unsafe CowSpeak-master/Exceptions/BaseException.cs CowSpeak-master/Exceptions/ConversionException.cs CowSpeak-master/Exceptions/ModuleException.cs CowSpeak-master/Modules/Linux.cs CowSpeak-master/Modules/Main.cs CowSpeak-master/Modules/ShorterTypeNames.cs CowSpeak-master/Modules/Windows.cs CowSpeak-master/Any.cs CowSpeak-master/AssemblyInfo.cs CowSpeak-master/ByteArray.cs CowSpeak-master/Conditional.cs CowSpeak-master/Conversion.cs CowSpeak-master/csharp-CowConfig.cs CowSpeak-master/Definition.cs CowSpeak-master/Executor.cs CowSpeak-master/Function.cs CowSpeak-master/FunctionChain.cs CowSpeak-master/Functions.cs CowSpeak-master/Interpreter.cs CowSpeak-master/Lexer.cs CowSpeak-master/Line.cs CowSpeak-master/Module.cs CowSpeak-master/ModuleSystem.cs CowSpeak-master/Scope.cs CowSpeak-master/StaticFunction.cs CowSpeak-master/Syntax.cs CowSpeak-master/Token.cs CowSpeak-master/Type.cs CowSpeak-master/UserFunction.cs CowSpeak-master/Utils.cs CowSpeak-master/Variable.cs CowSpeak-master/Variables.cs DynamicExpresso.Core/Exceptions/AssignmentOperatorDisabledException.cs DynamicExpresso.Core/Exceptions/DuplicateParameterException.cs DynamicExpresso.Core/Exceptions/DynamicExpressoException.cs DynamicExpresso.Core/Exceptions/NoApplicableMethodException.cs DynamicExpresso.Core/Exceptions/ParseException.cs DynamicExpresso.Core/Exceptions/ReflectionNotAllowedException.cs DynamicExpresso.Core/Exceptions/UnknownIdentifierException.cs DynamicExpresso.Core/Parsing/Parser.cs DynamicExpresso.Core/Parsing/ParserConstants.cs DynamicExpresso.Core/Parsing/ParserSettings.cs DynamicExpresso.Core/Parsing/ParseSignatures.cs DynamicExpresso.Core/Parsing/Token.cs DynamicExpresso.Core/Parsing/TokenId.cs DynamicExpresso.Core/Reflection/ReflectionExtensions.cs DynamicExpresso.Core/Resources/ErrorMessages.Designer.cs DynamicExpresso.Core/Visitors/DisableReflectionVisitor.cs DynamicExpresso.Core/AssignmentOperators.cs DynamicExpresso.Core/Detector.cs DynamicExpresso.Core/Identifier.cs DynamicExpresso.Core/IdentifiersInfo.cs DynamicExpresso.Core/Interpreter.cs DynamicExpresso.Core/InterpreterOptions.cs DynamicExpresso.Core/Lambda.cs DynamicExpresso.Core/LanguageConstants.cs DynamicExpresso.Core/Parameter.cs DynamicExpresso.Core/ParserArguments.cs DynamicExpresso.Core/ReferenceType.cs # Build as dll

# Run external CowSpeak shell referencing CowSpeak library
mcs -r:CowSpeak.dll -out:Shell.exe main.cs
mono Shell.exe
rm Shell.exe