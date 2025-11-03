# 1.2
## Fixed errors when working with the replace() method of the `json` type

- [replace from variable](tests/Lira.IntegrationTests/fixtures/rules/generating/types/json/replace.from_var.rules)

- [replace from csharp block from variable](tests/Lira.IntegrationTests/fixtures/rules/generating/types/json/replace.cs.from_var.rules)

- [replace from csharp inline block](tests/Lira.IntegrationTests/fixtures/rules/generating/types/json/replace.cs.objects.rules)

- [replace from inline value](tests/Lira.IntegrationTests/fixtures/rules/generating/types/json/replace.rules)

## Implemented the ability to store data within the processing of one request

- [example](tests/Lira.IntegrationTests/fixtures/rules/bag/chain.rules)

## Implemented the ability to perform matching based on all request data

- [via value() function](tests/Lira.IntegrationTests/fixtures/rules/match/csharp/req/values.rules)

- [via request data](tests/Lira.IntegrationTests/fixtures/rules/match/csharp/req/access_to_all_request_data.rules)

## Implemented state saving for `seq` and `range` functions

To save state between container restarts, you need to mount the volume to `/tmp/lira`

```
docker run -p 80:8080 -v c:/lira/rules:/app/rules -v c:/lira/data:/tmp/lira mnkvsoft/lira
```

## The ability to use `using` blocks in the rules has been implemented 

- [namespace import](tests/Lira.IntegrationTests/fixtures/rules/csharp/using/matching.rules)


- [static class import](tests/Lira.IntegrationTests/fixtures/rules/csharp/using/static.rules)

- [alias](tests/Lira.IntegrationTests/fixtures/rules/csharp/using/alias.rules)

## Connecting third party libraries

By default, the search is performed in the rules directory. If you need to search in another directory, you need to specify the `LibsPath` parameter. The search is performed using the mask `*.dll`:

```
docker run -p 80:8080 -e LibsPath=/tmp/lira/libs -v c:/lira/rules:/app/rules -v c:/lira/data:/tmp/lira mnkvsoft/lira
```
- [using in external `.cs` file](tests/Lira.IntegrationTests/fixtures/rules/third_party_libs/separate_cs_file.rules),
[`.cs` file ](tests/Lira.IntegrationTests/fixtures/rules/third_party_libs/StringUtils.cs)

- [in rule](tests/Lira.IntegrationTests/fixtures/rules/third_party_libs/in_rule.rules)

## Implemented the ability to use dynamic expressions in block `~ code`

- [example](tests/Lira.IntegrationTests/fixtures/rules/generating/http_code_block/1.rules)

## Simplified response description

- [code as section key](tests/Lira.IntegrationTests/fixtures/rules/generating/simplified_notation/code_as_section_key.rules)

- [code without block](tests/Lira.IntegrationTests/fixtures/rules/generating/simplified_notation/code_without_block.rules)

- [response body without blocks](tests/Lira.IntegrationTests/fixtures/rules/generating/simplified_notation/response_body_without_blocks.rules)

## Disabled certificate validation in section `action.call.http`

# 2.0

rename float => dec
add local variables
add write local and rule variables
add pan type for ranges
add ordered actions
nuget
add global usings
rename function prefix: $ => @
unorder declaration variables and functions
add declared items in random function
add operators support
add support remote git repository for download rules
forbid . character in a variable name
add log() function
comments: ## => @- ### ### => @* *@
improved readability of error messages
intervals: - => ..
dic() method for access from c# code

json:
- add json() extensions
- add read value property
- add transform methods
- add remove() method