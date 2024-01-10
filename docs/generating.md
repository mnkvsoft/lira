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

GET /hello/{{ name=person any }}

----- response

~ code
200

~ body
hello {{ req.path: person }}!
```



## req.body.jpath
Используется для извлечения данных из тела в формате `json` с использованием языка [JSON Path](addition_info.md#json-path).

#### Синтаксис
```
req.body.jpath: <JSON_Path_выражение>
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
req.body.xpath: <XPath_выражение>
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
form: <наименование_параметра>
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
Работа с функцией подробно описана в разделе [Сопоставление запросов с помощью интервалов](ranges.md)




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
Если в шаблоне требуется изменить какое-либо значение, то для функции нужно явно
задать тип `json` и при обращении к функции вызвать метод `replace()`, передав
[JSON Path](addition_info.md#json-path) нужного элемента и новое значение.

Подробнее о типе [json](types.md) 


#### Пример. Изменения значения в шаблоне
[template.json.rules](examples/guide/custom_functions/template.json.rules)
```
-------------------- rule

GET /orders

~ headers
example: guide/custom_functions/template.json

----- declare

$order.template:json = 
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
        {{ 
            $order.template
                .replace("$.status", "ok")
                .replace("$.customer", "vasily pupkin")
        }},
        {{ 
            $order.template
                .replace("$.status", "pending")
                .replace("$.customer", "john silver")
        }},
        {{ 
            $order.template
                .replace("$.status", "declined")
        }}
    ]
}
```
:triangular_flag_on_post: Блок, в котором используется функция `replace()` является
блоком `C#` - кода, который будет рассмотрен ниже. При вызовах функций в C# - блоках
символ `$` используемый при объявлении функций не может быть опущен


Запрос
```
curl --location 'http://localhost/orders' \
--header 'example: guide/custom_functions/template.json'
```
Ответ
```
{
    "orders": [{
            "id": 1001723617,
            "status": "ok",
            "amount": 6502.18,
            "transaction_id": "d4bec449-5ad6-448e-ad53-66d27cb86896",
            "created_at": "05/22/2023 18:51:55",
            "customer": "vasily pupkin"
        }, {
            "id": 679910945,
            "status": "pending",
            "amount": 5222.76,
            "transaction_id": "158e54b3-ff01-48ab-947a-71b4c4bd3860",
            "created_at": "06/05/2023 19:13:12",
            "customer": "john silver"
        }, {
            "id": 679102371,
            "status": "declined",
            "amount": 1112.33,
            "transaction_id": "43ff5b03-3348-4aed-a0f5-60988e597863",
            "created_at": "11/09/2023 15:10:41",
            "customer": "j886xeturw9lnss1h3c6"
        }
    ]
}
```

#### Пример. Определения функции для герерации специфичного значения
[int.big.rules](examples/guide/custom_functions/int.big.rules)
```
-------------------- rule

GET /order

~ headers
example: guide/custom_functions/int.big

----- declare

$int.big = {{ int: [1000000000 - 9999999999] }}

----- response

~ body
{
    "id": {{ int.big }}
}
```
Запрос
```
curl --location 'http://localhost/order' \
--header 'example: guide/custom_functions/int.big'
```
Ответ
```
{
    "id": 5941542837
}
```

#### Пример. Переопределения системных функций
Заменим системную функцию [int](#int), сузив диапазон выдаваемых значений 

[int.override.rules](examples/guide/custom_functions/int.override.rules)
```
-------------------- rule

GET /order

~ headers
example: guide/custom_functions/int.override

----- declare

$int = {{ int: [1 - 10] }}

----- response

~ body
{
    "items_count": {{ int }}
}
```
Запрос
```
curl --location 'http://localhost/order' \
--header 'example: guide/custom_functions/int.override'
```
Ответ
```
{
    "items_count": 2
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

Переменные регистрируются в секции `declare` с использованием суффикса `$$`

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

Переменные и функции могут быть задекларированы на следующих уровнях:
- на уровне правила, если требуется использование только в одном правиле
- на уровне файла, если требуется использование в нескольких правилах в одном файле
- на глобальном уровне, если требуется использование в любом правиле

В зависимости от требуемого уровня декларирование будет различаться.

### Декларирование на уровне правила
В этом случае блок `declare` располагается внутри секции `response`, 
на одном уровне с секцией response  

#### Пример
[rule.rules](examples/guide/declare_layers/rule.rules)
```
-------------------- rule

GET /order

~ headers
example: guide/declare_layers/rule

----- declare

$amount = {{ float: [100 - 10000] }}
$$id = {{ seq }}

----- response

~ body
{
    "transaction_id": {{ $$id }}
    "items":[
        {
            "parent_transaction_id": {{ $$id }},
            "amount": {{ $amount }}
        },
        {
            "parent_transaction_id": {{ $$id }},
            "amount": {{ $amount }}
        }
    ]
}
```
Запрос
```
curl --location 'http://localhost/order' \
--header 'example: guide/declare_layers/rule'
```
Ответ
```
{
    "transaction_id": 17
    "items":[
        {
            "parent_transaction_id": 17,
            "amount": 8543.10
        },
        {
            "parent_transaction_id": 17,
            "amount": 2459.21
        }
    ]
}
```

### Декларирование на уровне файла
В этом случае секция `declare` располагается на одном уровне с секцией `rule`

#### Пример
[file.rules](examples/guide/declare_layers/file.rules)
```
-------------------- declare

$amount = {{ float: [100 - 10000] }}
$$id = {{ seq }}
$template = 
{
    "id": {{ $$id }}
    "items":[
        {
            "parent_id": {{ $$id }},
            "amount": {{ $amount }}
        },
        {
            "parent_id": {{ $$id }},
            "amount": {{ $amount }}
        }
    ]
}

-------------------- rule

GET /order/1

~ headers
example: guide/declare_layers/file

----- response

~ body
{{ $template }}

-------------------- rule

GET /order/2

~ headers
example: guide/declare_layers/file

----- response

~ body
{{ $template }}
```
Запрос для 1 правила
```
curl --location 'http://localhost/order/1' \
--header 'example: guide/declare_layers/file'
```
Ответ
```
{
    "id": 26
    "items":[
        {
            "parent_id": 26,
            "amount": 9253.64
        },
        {
            "parent_id": 26,
            "amount": 2794.01
        }
    ]
}
```
Запрос для 2 правила
```
curl --location 'http://localhost/order/2' \
--header 'example: guide/declare_layers/file'
```
Ответ
```
{
    "id": 27
    "items":[
        {
            "parent_id": 27,
            "amount": 5022.35
        },
        {
            "parent_id": 27,
            "amount": 7377.37
        }
    ]
}
```
### Декларирование на глобальном уровне
Декларирование на глобильном уровне производится в файлах `*.declare`.
Файл может быть определен в любом месте рабочего каталога.

#### Пример
[declare.shared.global.declare](examples/quick_start/declare.shared.global.declare)
```
$age = {{ int: [1 - 122]}}
```
[global.rules](examples/quick_start/declare.shared.global.rules)

```
-------------------- rule

GET /person

~ headers
example: declare.shared.global

----- response

~ code
200

~ body
{
    "age": {{ age }}
}
```

Запрос
```
curl --location 'http://localhost/person' \
--header 'example: declare.shared.global'
```
Ответ
```
{
    "age": 64
}
```


## Порядок определения переменных и функций
В пользовательский переменных и функциях можно использовать другие пользовательские
переменные и функции, но они должные быть задекларированы ранее.

:triangular_flag_on_post: Порядок декларирования имеет значение.

### Порядок загрузки переменных и функций
Сначала считываются все файлы `*.declare` в алфавитном порятки, 
далее производится считывание из блоков `declare` на уровне файла, 
далее производится чтение блоков `declare` на уровне правил.

:triangular_flag_on_post: Не допускается использование определения функций 
с одинаковыми названиями на любых уровнях (т.е. не может быть функции `age` 
определенной на глобальном уровне и на уровне файла (или правила)), это справедлива
и для переменных. Если подобное случится, то сервер выдаст соответствующую ошибку

# Блоки кода на языке C#

В некоторых случаях функциональности встроенных функций не хватает для описания
специфичной логики. В этом случае можно использовать блоки кода на языке C#.
Блоке на языке c# допустимы в любых динамических блоках, как непосредственно
при генерации ответа в теле запроса и заголовках, так в переменных и функциях.

Блоки делятся на 2 типа:
- короткие
- полные

Ниже рассматриваются оба типа.


## Короткие блоки
Подразумевают инструцию без использования дополнительных переменных и
возвращающую **не** `void` значение

[short.rules](examples/quick_start/charp.short.rules)
```
-------------------- rule

GET /very/old/event

----- response

~ code
200

~ body
{
    "date": {{ DateTime.Now.AddYears(-1000 - Random.Shared.Next(1, 100)) }}
}
```
Запрос
```
curl --location 'http://localhost/very/old/event'
```
Ответ
```
{
    "date": 0926-11-12T14:14:55.0009386+02:31
}
```

## Полные блоки
Полные блоки кода используются, если необходимо написать какую-то логику, 
трубующую введения дополнительных переменных.

#### Пример. Блок C# - кода при генерации тела сообщения 

[full.rules](examples/guide/csharp/full.rules)
```cs
-------------------- rule

GET /order

~ headers
example: guide/charp/full

----- response

~ body
{
    "customer": "{{ 
        var firstNames = new []{"Vasily", "Nikolas", "Ivan", "John"};
        var lastNames = new []{"Pupkin", "Stallone", "Norris", "Ivanov"};

        var firstName = firstNames[Random.Shared.Next(0, firstNames.Length - 1)];
        var lastName = lastNames[Random.Shared.Next(0, lastNames.Length - 1)];

        var name = firstName + " " + lastName;
        return name;
     }}"
}
```
Запрос
```
curl --location 'http://localhost/order' \
--header 'example: guide/charp/full'
```
Ответ
```
{
    "customer": "Vasily Stallone"
}
```

#### Пример. Блок C# - кода в пользовательской функции

[full.function.rules](examples/guide/csharp/full.function.rules)
```cs
-------------------- rule

GET /order

~ headers
example: guide/charp/full.function

----- declare
$name = 
{{
     var firstNames = new []{"Vasily", "Nikolas", "Ivan", "John"};
        var lastNames = new []{"Pupkin", "Stallone", "Norris", "Ivanov"};

        var firstName = firstNames[Random.Shared.Next(0, firstNames.Length - 1)];
        var lastName = lastNames[Random.Shared.Next(0, lastNames.Length - 1)];

        var name = firstName + " " + lastName;
        return name;
}}
----- response

~ body
{
    "customer": "{{ $name }}"
}
```
Запрос
```
curl --location 'http://localhost/order' \
--header 'example: guide/charp/full.function'
```
Ответ
```
{
    "customer": "Nikolas Norris"
}
```






## Доступ к параметрам запроса
Доступ к параметрам запроса для коротких и полных блоков можно получить с помощью
системной переменной `req`.

Ниже описаны методы этой переменной.

### query
Извлекает значение параметра строки запроса
**Синтаксис**
```
req.query("<имя_параметра>")
```

### header
Извлекает значение заголовка
**Синтаксис**
```
req.header("<имя_заголовка>")
```

### path
Извлекает значение сегмента по его имени. Используется только для сегмента или его части, для которого определено динамическое сопоставление
**Синтаксис**
```
req.path("<имя_сегмента_пути>")
```

### body.jPath
Используется для извлечения данных из тела в формате `json` 
с использованием языка [JSON Path](addition_info.md#json-path).

**Синтаксис**
```
req.body.jpath("<JSON_Path_выражение>")
```


### body.xPath
Используется для извлечения данных из тела в формате `xml` 
с использованием языка [XPath](https://ru.wikipedia.org/wiki/XPath).

**Синтаксис**
```
req.body.xpath("<JSON_Path_выражение>")
```

### body.form
Используется для извлечения данных из тела в формате `x-www-form-urlencoded`.

**Синтаксис**
```
req.body.form("<наименование_параметра>")
```

### body.all
Извлекает все тело запроса

**Синтаксис**
```
req.body.all()
```


## Использование переменных и функций в блоках C#




## Форматирование

Любое значение полученное из динамического выражения может быть отформатировано
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
Оператор `>>` передает значение полученное динамической функцией на вход функции
`format`, которая выполняет форматирование. Оператор `>>` подробнее рассматривается в разделе 
[Модель передачи значения по цепочки функций](#модель-передачи-значения-по-цепочки-функций).



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
Форматирование разных типов данных основано на общем механизме форматирования
**.NET**

Подробное описание форматов для разных типов данных можно найти по
[ссылке](https://learn.microsoft.com/ru-ru/dotnet/standard/base-types/formatting-types).

Описание форматирования для различных типов данных:
- [Числовые](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings)
- [Guid](https://learn.microsoft.com/ru-ru/dotnet/api/system.guid.tostring?view=net-7.0)
- [DateTime](https://learn.microsoft.com/ru-ru/dotnet/standard/base-types/custom-date-and-time-format-strings)


# Раздел находится в стадии написания...


## Модель передачи значения по цепочки функций

- переменные
- пользовательские функции
- блоки кода на языке C#
