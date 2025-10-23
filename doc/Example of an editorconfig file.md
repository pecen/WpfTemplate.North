
If an editorconfig file should be used to control what editor settings in Visual Studio developers should be bound to, do the following before you create the main solution in Visual Studio:

1. In the root folder for the solution, i.e. the src folder, create a file with the name .editorconfig (don't miss the dot in the beginning of the filename)
2. Copy and paste the following (beware that this is only an example and that you might have other requirements):

```
root = true

[*]
end_of_line = crlf
tab_width=4
indent_size=4

[*.cs]
indent_style = tab

csharp_new_line_before_catch = true
csharp_new_line_before_else = true
csharp_new_line_before_finally = true
csharp_new_line_before_members_in_anonymous_types = true
csharp_new_line_before_members_in_object_initializers = true
csharp_new_line_before_open_brace = true
csharp_new_line_between_query_expression_clauses = true

dotnet_naming_rule.private_members_with_underscore.symbols  = private_fields
dotnet_naming_rule.private_members_with_underscore.style    = prefix_underscore
dotnet_naming_rule.private_members_with_underscore.severity = suggestion

dotnet_naming_symbols.private_fields.applicable_kinds           = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private

dotnet_naming_style.prefix_underscore.capitalization = camel_case
dotnet_naming_style.prefix_underscore.required_prefix = _

[*.xaml]
indent_style = tab

[*.xml]
indent_style = tab
```

3. Save and close the file
