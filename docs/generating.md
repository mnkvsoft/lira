# Генерация ответов

Для описания ***блока выходных данных*** используется произвольный текст
со вставками ***динамических выражений***

#### Пример. Блок выходных данных
```
This event happened "{{ date }}" on the street "{{ str }}". It was raining heavily, it was getting dark... 
```

***Динамические выражения*** описываются внутри блоков `{{ }}`. 
В данном примере в качестве динамических выражений было использовано 2 системные функции: `date`, `str`.

Динамические выражения рассматриваются [ниже](#динамические-выражения)
 
## Тело ответа
***Блок выходных данных*** тела ответа описывается в блоке `body`

#### Пример

[body.rules](examples/generating/body.rules)
```
-------------------- rule

GET /story

----- response

~ code
200

~ body
This event happened "{{ date }}" on the street "{{ str }}". It was raining heavily, it was getting dark...
```
Запрос
```
curl --location 'http://localhost/story'
```
Ответ
```
This event happened "2016-09-21T16:11:56.3319249" on the street "0ljk04kwwor93crz9p3p". It was raining heavily, it was getting dark...
```


## Заголовки ответа

Заголовки ответка описываются в блоке `headers`

#### Синтаксис
```
<заголовок_1>: <блок_выходных_данных_1>
...
<заголовок_N>: <блок_выходных_данных_N>
```

#### Пример

[headers.rules](examples/generating/headers.rules)
```
-------------------- rule

GET /story

~ headers
example: headers

----- response

~ code
200

~ headers
Story: This event happened "{{ date }}" on the street "{{ str }}". It was raining heavily, it was getting dark...
Request-Id: {{ guid }}
```
Запрос
```
curl --location 'http://localhost/story' \
--header 'example: headers'
```
Ответ
```
Story: This event happened "2022-12-09T04:25:25.0212392" on the street "8h6cubx047f10kn5tr19". It was raining heavily, it was getting dark...
Request-Id: 512de6f0-e241-45c3-a012-b21086751e0d
```


## Код ответа

Код ответа может быть указан следующими способами:
- в блоке `code`
- без блока, в секции `response`
- не указан, в этом случае используется значение `200`

#### Пример. В блоке `code`

[http_code_in_block.rules](examples/generating/http_code_in_block.rules)
```
-------------------- rule

POST /order

~ headers
example: http_code_in_block

----- response

~ code
403
```
Запрос
```
curl --location --request POST 'http://localhost/order' \
--header 'example: http_code_in_block'
```
Ответ
```
403
```


#### Пример. В секции `response`

[http_code_in_section.rules](examples/generating/http_code_in_section.rules)
```
-------------------- rule

POST /order

~ headers
example: http_code_in_section

----- response

403
```
Запрос
```
curl --location --request POST 'http://localhost/order' \
--header 'example: http_code_in_section'
```
Ответ
```
403
```

#### Пример. Не указан

[http_code_default.rules](examples/generating/http_code_default.rules)
```
-------------------- rule

POST /order

~ headers
example: http_code_default

----- response

~ body
{
    "status": "ok"
}
```
Запрос
```
curl --location --request POST 'http://localhost/order' \
--header 'example: http_code_default'
```
Ответ
```
{
    "status": "ok"
}
```

# Динамические выражения

Динамеские выражения могут быть разных типов, ниже рассматривается каждый из них

## Форматирование 

Любое значение полученное из динамического выражения 

Все сгенерированные данные данные могут быть отформатированы,
с помощью функции `format`, если возвращенный тип поддерживает форматирование

#### Пример
[format.rules](examples/generating/format.rules)

```
-------------------- rule

GET /generating/format

----- response

~ body
{{ date >> format: MM/dd/yyyy }}
{{ int >> format: #.00# }}
{{ guid >> format: N }}
```
Запрос
```
curl --location 'http://localhost/generating/format'
```
Ответ
```
07/25/2021
472784009.00
d953d01ce7f640119f36ad7cae1668ba
```



# Предопреденные системные функции
Системные функции делятся на 2 группы:
- извлекающие данные запроса
- генерирующие данные

Ниже будут рассмотрены обе группы

## Функции извлекающие данные запроса

## req.query
Извлекает значение параметра строки запроса
#### Синтаксис
```
req.query: <имя_параметра>
```

## req.header
Извлекает значение заголовка
#### Синтаксис
```
req.header: <имя_заголовка>
```


## req.path
Извлекает значение сегмента по его имени. 
Используется только для сегмента или его части, для которого определено динамическое
сопоставление
#### Синтаксис
```
req.path: <имя_сегмента_пути>
```
#### Пример
[hello.rules](examples/quick_start/hello.rules)
```
-------------------- rule

GET /hello/{{ any name: person }}

----- response

~ code
200

~ body
hello {{ req.path: person }}!
```



## req.body.jpath
Используется для извлечения данных из тела в формате `json` с использованием языка `JSON Path`.
Ознакомится с коротким описанием можно, например, по [ссылке](https://goessner.net/articles/JsonPath/)

Для обеспечения данной функциональности используется библиотека [newtonsoft](https://www.newtonsoft.com/json/help/html/QueryJsonSelectToken.htm)

#### Синтаксис
```
req.body.jpath: <JSON Path выражение>
```

#### Пример
[body.jpath.rules](examples/generating/body.jpath.rules)
```
-------------------- rule

POST /order

~ headers
example: body.jpath

----- response

~ body
{{ req.body.jpath: $.amount }}
```
Запрос
```
curl --location 'http://localhost/order' \
--header 'example: body.jpath' \
--header 'Content-Type: application/json' \
--data '{
    "amount": 124
}'
```
Ответ
```
124
```



## req.body.xpath
Используется для извлечения данных из тела в формате `xml` с использованием языка [XPath](https://ru.wikipedia.org/wiki/XPath).

#### Синтаксис
```
req.body.xpath: <XPath выражение>
```

#### Пример

[body.xpath.rules](examples/generating/body.xpath.rules)
```
-------------------- rule

POST /order

~ headers
example: body.xpath

----- response

~ body
{{ req.body.xpath: /order/amount/text() }}
```
Запрос
```
curl --location 'http://localhost/order' \
--header 'example: body.xpath' \
--header 'Content-Type: application/xml' \
--data '<order>
    <amount>124</amount>
</order>'
```
Ответ
```
124
```





## req.body.form
Используется для извлечения данных из тела в формате `x-www-form-urlencoded`

#### Синтаксис
```
form: <наименование параметра>
```

#### Пример

[body.form.rules](examples/generating/body.form.rules)
```
-------------------- rule

POST /order

~ headers
example: body.form

----- response

~ body
{{ req.body.form: amount }}
{{ req.body.form: description }}
```
Запрос
```
curl --location 'http://localhost/order' \
--header 'example: body.form' \
--header 'Content-Type: application/x-www-form-urlencoded' \
--data-urlencode 'amount=123.56' \
--data-urlencode 'description=it'\''s very important order'
```
Ответ
```
123.56
it's very important order
```




## req.body.all
Извлекает все тело запроса

#### Пример

[body.all.rules](examples/generating/body.all.rules)
```
-------------------- rule

POST /order

~ headers
example: body.all

----- response

~ body
request body: {{ req.body.all }}
```
Запрос
```
curl --location 'http://localhost/order' \
--header 'example: body.all' \
--header 'Content-Type: text/plain' \
--data 'some description'
```
Ответ
```
request body: some description
```



## Функции генерирующие данные

Функции геренерирующие даты по умолчанию возвращают данные в формате ISO_8601. 
Подробнее с форматом можно ознакомиться по [ссылке](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#Roundtrip)

## now
Возвращает текущую дату в локальном времени

## now.utc
Возвращает текущую дату в UTC

## date
Возвращает дату в локальном времени в интервале `[текущая дата - 1 год] - [текущая дата]` 

## date.past
Возвращает дату в локальном времени в интервале `[текущая дата - 11 лет] - [текущая дата - 1 год]`

## date.future
Возвращает дату в локальном времени в интервале `[текущая дата + 1 год] - [текущая дата + 11 лет]`

## date.utc
Возвращает текущую дату в UTC в интервале `[текущая дата - 1 год] - [текущая дата]`

## date.utc.past
Возвращает дату в UTC в интервале `[текущая дата - 11 лет] - [текущая дата - 1 год]`

## date.utc.future
Возвращает дату в UTC в интервале `[текущая дата + 1 год] - [текущая дата + 11 лет]`



## int
Возвращает случайное целочисленное значение

#### Синтаксис
```
int[: <интервал>]
```

Если интервал не указан, то по умолчанию используется `1 - 2 147 483 647`.

Подробнее об определении интервалов в [разделе](guide.md#описание-интервалов)



## float
Возвращает случайное число с 2 знаками после запятой

#### Синтаксис
```
float[: <интервал>]
```

Если интервал не указан, то по умолчанию используется `0.01 - 10000`.

Подробнее об определении интервалов в [разделе](guide.md#описание-интервалов)



## guid 
Возвращает GUID. Формат по умолчанию: `00000000-0000-0000-0000-000000000000`

## str
Возвращает случайную строку. 

Строка формируется из следующих символов
```
1234567890
qwertyuiop
asdfghjkl
zxcvbnm
```

#### Синтаксис
```
str[: <длина строки>]
```
Если длина строки не указана, то по умолчанию используется значение `20`

## seq
Последовательно возвращает значения в диапазоне `1 - 9 223 372 036 854 775 807`

## range

Возвращает значение из диапазона определенного в файле `*.ranges.json`
#### Синтаксис
```
range: <имя_диапазона>/<диапазон> 
```
Данная функция используется в сложных сценариях сопоставления,
где необходимо выдавать разные ответы при запросе на одну и ту же конечную точку.
Работа с функцией подробно описана в разделе [Сопоставление запросов с помощью интервалов](data.md)




# Пользовательские функции

Пользовательские функции используются для:
- объединения повторяющегося кода
- для определения шаблонов, используемых в теле ответа. 
Шаблон может быть определен на уровне правила, файла или глобально. 
[Подробнее об уровнях декларирования](#уровни-декларирования-переменных-и-пользовательских-функций) 
- определения собственных функций, если требуется специфичная логика герерации значения.
Для этого могут быть использованы либо системные функции, либо код на языке C#.
[Подробнее о блоках кода на языке C#](#блоки-кода-на-языке-c)
- переопределения системных функций

Пользовательские функции декларируются в блоке `declare` с успользованием суффикса `$`.
При вызове пользовательской функции символ `$` может быть опущен.

В именах можно использовать символ `.`


#### Пример. Объединение повторяющегося кода
[req.id.rules](examples/guide/custom_functions/req.id.rules)
```
-------------------- rule

GET /payment

~ headers
example: guide/custom_functions/req.id
Request-Id: {{ any }}

----- declare

$req.id = {{ req.header: Request-Id }}

----- response

~ headers
Request-Id: {{ $req.id }}

~ body
{
    "id": {{ seq }},
    "request_id": {{ $req.id }}
}
```

Запрос
```
curl --location 'http://localhost/payment' \
--header 'example: guide/custom_functions/req.id' \
--header 'Request-Id: 12345'
```
Ответ
```
Request-Id: 12345

{
    "id": 1,
    "request_id": 12345
}
```



При вызове пользовательских функций символ `$` может быть опущен.

В этом случае правило будет выглядеть следующим образом:

```
-------------------- rule

GET /payment

~ headers
example: guide/custom_functions/req.id
Request-Id: {{ any }}

----- declare

$req.id = {{ req.header: Request-Id }}

----- response

~ headers
Request-Id: {{ req.id }}

~ body
{
    "id": {{ seq }},
    "request_id": {{ req.id }}
}
```

#### Пример. Определение шаблонов
[template.rules](examples/guide/custom_functions/template.rules)
```
-------------------- rule

GET /orders

~ headers
example: guide/custom_functions/template

----- declare

$order.template = 
{
    "id": {{ int }},
    "status": "paid",
    "amount": {{ float }},
    "transaction_id": "{{ guid }}",
    "created_at": "{{ date }}",
    "customer": "{{ str }}"
}

----- response

~ body
{
    "orders": [
        {{ $order.template }},
        {{ $order.template }},
        {{ $order.template }}
    ]
}
```
Запрос
```
curl --location 'http://localhost/orders' \
--header 'example: guide/custom_functions/template'
```
Ответ
```
{
    "orders": [{
            "id": 408632577,
            "status": "paid",
            "amount": 4173.53,
            "transaction_id": "99feb828-2835-467c-9e7f-c48551aa2568",
            "created_at": "06/11/2015 15:17:40",
            "customer": "v1xb4qafbhsn4s225jkz"
        }, {
            "id": 1959558077,
            "status": "paid",
            "amount": 8702.35,
            "transaction_id": "2358293b-9886-43dc-a5d2-ac4157f5a883",
            "created_at": "12/22/2018 10:15:49",
            "customer": "12hp41a9r7k9wjodztul"
        }, {
            "id": 859052464,
            "status": "paid",
            "amount": 621.20,
            "transaction_id": "d1796b75-d726-4500-a99c-93f53bf72607",
            "created_at": "07/24/2018 06:22:22",
            "customer": "bgdo8sbq7ag6j8koyadg"
        }
    ]
}

```

# Переменные
Если одно и то же сгенерированое значение необходимо использовать при 
формировании ответа несколько раз, то для этих целей используются 
переменные.

Переменные часто используется при осуществлении [обратных вызовов](callback.md), 
когда необходимо одно и то же значение 
идентификатора выдать в ответе на запрос и передать в запрос
на обратный вызов.

Переменные регистрируются в секции *declare* с использованием суффикса `$$`

[variables.rules](examples/generating/variables.rules)
```
-------------------- rule

POST /payment

~ headers
example: generating/variables

----- declare

$$requestId = {{ guid }}

----- response

~ code
200

~ headers
Request-Id: {{ $$requestId }}

~ body
{
    "request_id": "{{ $$requestId }}"
}
```
Запрос
```
curl --location --request POST 'http://localhost/payment' \
--header 'example: generating/variables'
```

Ответ
```
Request-Id: 4af7cf3b-e6c4-43ef-aa43-000c883cd081

{
    "request_id": "4af7cf3b-e6c4-43ef-aa43-000c883cd081"
}
```


## Уровни декларирования переменных и пользовательских функций

## Форматирование

Любое значение полученное из динамического выражения

Все сгенерированные данные данные могут быть отформатированы,
с помощью функции `format`, если возвращенный тип поддерживает форматирование

#### Пример
[format.rules](examples/generating/format.rules)

```
-------------------- rule

GET /generating/format

----- response

~ body
{{ date >> format: MM/dd/yyyy }}
{{ int >> format: #.00# }}
{{ guid >> format: N }}
```
Запрос
```
curl --location 'http://localhost/generating/format'
```
Ответ
```
07/25/2021
472784009.00
d953d01ce7f640119f36ad7cae1668ba
```


## Блоки кода на языке C#



- переменные
- пользовательские функции
- блоки кода на языке C#