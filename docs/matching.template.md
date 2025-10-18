# Cопоставления запросов

**LIRA** позволяет настроить правила по следующим параметрам запроса:
- метод (`GET`, `PUT`, `POST`, `DELETE`, `HEAD`, `TRACE`, `OPTIONS`, `CONNECT`)
- путь 
- параметры строки запроса
- заголовки
- тело

#### Пример
[static.rules](examples/guide/static.rules)

Запрос
```
curl --location 'http://localhost/payment/card?amount=100' \
--header 'example: static' \
--header 'Content-Type: text/plain' \
--data 'number=1111222233334444'
```

Приведенное выше правило будет срабатывать при выполнении всех следующих условий:
- запрос выполен методом `POST`
- запрос выполнен по пути `/payment/card`
- в параметрах строки запроса присутствует параметр `amount` со значением `100`. 
- наличие заголовка `example` со значением `static`
- в теле запроса содержится контент `number=1111222233334444`

Выше был приведен пример правила, где все параметры запроса заданы статично, далее будет описано динамическое сопоставление параметров запроса.
 


## Динамическое сопоставление параметров запроса

Шаблон сопоставления для любого параметра запроса выглядит следующим образом:

`[префикс]{{ <функция_сопосталения> }}[суффикс]`

***префикс*** - значение, с которого начинается сегмент

***суффикс*** - значение, на которое заканчивается сегмент

***функция сопосталения*** - одна из [функций сопоставления](#функции-сопоставления)

далее в тексте будет обозначаться как 

`[шаблон сопоставления]`

:triangular_flag_on_post: Шаблоны с несколькими функциями сопоставления не допускаются

#### Примеры

#### Не корректный шаблон сопосталения

`ivanov_{{ any }}_{{ any }}`

#### Корректный шаблон сопосталения

`ivanov_{{ any }}_petrovich`



## Функции сопоставления

### any
Соответствует любой строке

### int
Целое число в интервале `- 9 223 372 036 854 775 808` - `9 223 372 036 854 775 807` (64 бита)
#### Синтаксис
```
int[: <интервал>]
```
Если интервал не указан, то используется максимальный интервал

Подробнее об определении интервалов в [разделе](guide.md#описание-интервалов)

### float
Число, которое может иметь дробную часть. Интервал `±1.0*10^{-28}` - `±7.9228*10^{28}` (128 бит)
#### Синтаксис
```
float[: <интервал>]
```
Если интервал не указан, то используется максимальный интервал

Подробнее об определении интервалов в [разделе](guide.md#описание-интервалов)


### guid
GUID в любом формате

### regex 
Сопоставляет значение с указанными регулярным выпажением

#### Синтаксис
```
regex: <регулярное_выражение>
```
### range
Проверяет строку на принадлежность диапазону определенному в файле `*.ranges.json`
#### Синтаксис
```
range: <имя_диапазона>/<диапазон> 
```
Данная функция используется в сложных сценариях сопоставления, 
где необходимо выдавать разные ответы при запросе на одну и ту же конечную точку. 
Работа с функцией подробно описана в разделе [Сопоставление запросов с помощью интервалов](ranges.md)



## Сопоставление пути

Сегмент пути может быть задан либо статично, либо шаблоном сопоставления:

**/**`[[статичная часть 1] | [шаблон сопоставления 1]`**/.../**`[[статичная часть N] | [шаблон сопоставления N]]`

#### Примеры

#### C указанием начального и конечного значения в сегменте
[path_with_start_end.rules](examples/guide/path_with_start_end.rules)
Запросы подпадающие под правило
```
curl --location 'http://localhost/user/ivanov_petr_petrovich'
curl --location 'http://localhost/user/ivanov_ivan_petrovich'
```

#### C ипользованием только функции сопоставления
[path_with_one_func.rules](examples/guide/path_with_one_func.rules)
Запросы подпадающие под правило
```
curl --location 'http://localhost/user/123'
curl --location 'http://localhost/user/ivan_petrovich'
curl --location 'http://localhost/user/df1243'
curl --location 'http://localhost/user/ivan_petrovich'
```

#### C ипользованием функций в разных сегментах
[path_with_two_func.rules](examples/guide/path_with_two_func.rules)
Запросы подпадающие под правило
```
curl --location 'http://localhost/user/123/1'
curl --location 'http://localhost/user/ivan_petrovich/3'
```



## Сопоставление параметров строки строки

Параметр строки запроса может быть задан либо статично, либо шаблоном сопоставления:

`?[<параметр 1>=<[статичная часть 1] | [шаблон сопоставления 1]>]`**&...&**`[<параметр N>=<[статичная часть N] | [шаблон сопоставления N]>]`


#### Примеры



#### C ипользованием только функции сопоставления
[query_with_one_func.rules](examples/guide/query_with_one_func.rules)
Запросы подпадающие под правило
```
curl --location 'http://localhost/user?name=nikolas'
curl --location 'http://localhost/user?name=silvester'
```

:triangular_flag_on_post: Сопоставление по одному из параметров строки запроса
не исключает присутствия других параметров в запросе

Запросы также подпадающие под правило
```
curl --location 'http://localhost/user?name=nikolas?age=38'
curl --location 'http://localhost/user?name=silvester?height=177'
```



#### C указанием начального и конечного значения в параметре строки запроса
[query_with_start_end.rules](examples/guide/query_with_start_end.rules)
Запросы подпадающие под правило
```
curl --location 'http://localhost/user?name=anna'
curl --location 'http://localhost/user?name=alina'
```


#### C ипользованием функций в разных параметрах
[query_with_two_func.rules](examples/guide/query_with_two_func.rules)

Запросы подпадающие под правило

```
curl --location 'http://localhost/user?name=pavel&ago=36'
curl --location 'http://localhost/user?name=denis&ago=35&height=180'
```



## Сопоставление заголовков

Заголовок может быть задан либо статично, либо шаблоном сопоставления:

```
[<заголовок 1>: <[статичная часть 1] | [шаблон сопоставления 1]>]
...
[<заголовок N>: <[статичная часть 1] | [шаблон сопоставления 1]>]
```

#### Примеры


#### C ипользованием только функции сопоставления
[header_with_one_func.rules](examples/guide/header_with_one_func.rules)
Запросы подпадающие под правило
```
curl --location --request POST 'http://localhost/user' \
--header 'Request-Id: 3686D720-2926-4AC6-BFDC-029430828875'

curl --location --request POST 'http://localhost/user' \
--header 'Request-Id: 00000000-0000-0000-0000-000000000000'

curl --location --request POST 'http://localhost/user' \
--header 'Request-Id: 11111111-1111-1111-1111-111111111111'
```

:triangular_flag_on_post: Сопоставление по одномо из заголовков
не исключает присутствия заголовков в запросе

Запрос также подпадающий под правило
```
curl --location --request POST 'http://localhost/user' \
--header 'Request-Id: 3686D720-2926-4AC6-BFDC-029430828875' \
--header 'Token: 12345'
```


#### C ипользованием только функций сопоставления и статичного значения
[header_with_statuc_and_funcs.rules](examples/guide/header_with_statuc_and_funcs.rules)
Запросы подпадающие под правило
```
curl --location --request POST 'http://localhost/user' \
--header 'Token: 11__22' \
--header 'Request-Time: 14:21:01.123' \
--header 'No-Cache: true'

curl --location --request POST 'http://localhost/user' \
--header 'Token: 11_qwerty_22' \
--header 'Request-Time: 14:21' \
--header 'No-Cache: true'
```


## Сопоставление тела запроса

Тело запроса можно сопоставить либо по полному значению, либо извлечь части тела запроса,
в зависимости от формата (`json`, `xml`, `x-www-form-urlencoded`), и сопоставить по ним

### Сопоставление по полному значению
Тело запроса может быть задано либо статично, либо шаблоном сопоставления:
```
<[статичная часть] | [шаблон сопоставления]>
```

#### Примеры


#### C ипользованием только функции сопоставления
[body_with_one_func.rules](examples/guide/body_with_one_func.rules)
Запрос подпадающий под правило
```
curl --location 'http://localhost/user' \
--header 'example: body_with_one_func' \
--header 'Content-Type: text/plain' \
--data '12345'
```



### Сопоставление по извлеченной части 

Сопосталение части тела запроса выполняется по следующему шаблону

```
[<функция извлечения 1> >> <шаблон сопоставления 1>]
...
[<функция извлечения N> >> <шаблон сопоставления N>]
```

***функция извлечения*** - функция, которая в зависимости от формата тела запроса, 
извлечет часть данных

### Функции извлечения

### jpath

Используется для извлечения данных из тела в формате `json` с использованием языка `JSON Path`.
Ознакомится с коротким описанием можно, например, по [ссылке](https://goessner.net/articles/JsonPath/)

Для обеспечения данной функциональности используется библиотека [newtonsoft](https://www.newtonsoft.com/json/help/html/QueryJsonSelectToken.htm)

Синтаксис 
```
jpath: <JSON_Path_выражение>
```

#### Пример

[body_jpath.rules](examples/guide/body_jpath.rules)
Запрос подпадающий под правило
```
curl --location 'http://localhost/payment/card' \
--header 'example: body_jpath' \
--header 'Content-Type: application/json' \
--data '{
    "number": "1111222233334444",
    "owner": "Rodrygo",
    "amount": 123.99
}'
```



### xpath

Используется для извлечения данных из тела в формате `xml` с использованием языка [XPath](https://ru.wikipedia.org/wiki/XPath).

Синтаксис
```
xpath: <XPath_выражение>
```

#### Пример

[body_xpath.rules](examples/guide/body_xpath.rules)
Запрос подпадающий под правило
```
curl --location 'http://localhost/payment/card' \
--header 'example: body_xpath' \
--header 'Content-Type: application/xml' \
--data '<root>
    <number>1111222233334444</number>
    <owner>Rodrygo</owner>
    <amount>123.99</amount>
</root>'
```




### form

Используется для извлечения данных из тела в формате `x-www-form-urlencoded`

#### Синтаксис
```
form: <наименование_параметра>
```

#### Пример

[body_form.rules](examples/guide/body_form.rules)
Запрос подпадающий под правило
```
curl --location 'http://localhost/payment/card' \
--header 'example: body_form' \
--header 'Content-Type: application/x-www-form-urlencoded' \
--data-urlencode 'number=1111222233334444' \
--data-urlencode 'owner=Rodrygo'
```



## Операторы сравнения
Операторы сравнения используются для настройки [Сопоставления по условиям](#Сопоставление-по-условию) (описано ниже)

Поддерживаются следующие операторы `=`,`>`, `>=`,`<`,`<=`,`in`

Оператор `in` определяет диапазон, которому должна принадлежать переменная, 
с нижней и верхней границей включительно

#### Синтаксис
```
<переменная> in <диапазон>
```

#### Пример
```
attempt in [1 - 3]
```

Оператор `in` может представлен с помощью опереторов `>=` и `<=`

```
attempt >= 1
attempt <= 3
```


## Сопоставление по условию
Используется при необходимости выдавать разные варианты ответа на один и тот же запрос. 
Условия описываются в секции `condition`. Условия могут быть заданы либо по номеру попытки, 
либо по количеству времени прошедшему с момента первого запроса

В качестве данных, используемых для определения уникальности запроса, используются все данные запроса
(метод, путь, параметры строки запроса, заголовки, тело)


### Сопоставление по номеру попытки

Для сопоставления по номеру попытки используется системная переменная `attempt`

#### Синтаксис секции
```
[attempt <оператор сравнения 1> <номер попытки>]
...
[attempt <оператор сравнения N> <номер попытки>]
```

***оператор сравнения*** - описание [здесь](#операторы-сравнения)

#### Пример

```
---------------------------- rule

GET /payment/status

~ headers
example: conditions_attempt

--------------- condition

attempt < 2

----- response

~ code
200

~ body
{
    "status": "registered"
}

--------------- condition

attempt in [2 - 4]

----- response

~ code
200

~ body
{
    "status": "pending"
}


--------------- condition

attempt > 4

----- response

~ code
200

~ body
{
    "status": "ok"
}
```
Запрос 
```
curl --location 'http://localhost/payment/status' \
--header 'example: conditions_attempt'
```
Ответ на 1 запрос
```
{
    "status": "registered"
}
```
Ответ на 2 запрос
```
{
    "status": "pending"
}
```
Ответ на 3 запрос
```
{
    "status": "pending"
}
```
Ответ на 4 запрос
```
{
    "status": "pending"
}
```
Ответ на 5 запрос
```
{
    "status": "ok"
}
```


### Сопоставление по времени, прошедшему с момента первого запроса

Для сопоставления по времени используется системная переменная `elapsed`
#### Синтаксис секции
```
[elapsed <оператор сравнения 1> <временной интервал>]
...
[elapsed <оператор сравнения N> <временной интервал>]
```

***оператор сравнения*** - описание [здесь](#операторы-сравнения)

***временной интервал*** - описание [здесь](guide.md#описание-временных-интервалов)

#### Пример

```
---------------------------- rule

GET /payment/status

~ headers
example: conditions_elapsed

--------------- condition

elapsed < 2 seconds

----- response

~ code
200

~ body
{
    "status": "registered"
}

--------------- condition

elapsed in [2 second - 4 second]

----- response

~ code
200

~ body
{
    "status": "pending"
}


--------------- condition

elapsed > 4 second

----- response

~ code
200

~ body
{
    "status": "ok"
}
```
Запрос
```
curl --location 'http://localhost/payment/status' \
--header 'example: conditions_elapsed'
```
Ответ на 1 запрос
```
{
    "status": "registered"
}
```
После первого запроса прошло менее 2 секунд
```
{
    "status": "registered"
}
```
После первого запроса прошло от 2 до 4 секунд
```
{
    "status": "pending"
}
```
После первого запроса прошло более 4 секунд
```
{
    "status": "ok"
}
```


### Приоритизация правил
Одному запросу может соответствовать несколько правил,
LIRA выберет наиболее частное

[priority.rules](examples/quick_start/priority.rules)
Запрос
```
curl --location 'http://localhost/priority/1'
```
Ответ
```
rule with ANY
```

Запрос
```
curl --location 'http://localhost/priority/6EC35301-1CF0-4F9B-8472-8CD8EBA02105'
```
Ответ
```
rule with GUID
```

Запрос
```
curl --location 'http://localhost/priority/6EC35301-1CF0-4F9B-8472-8CD8EBA02105' \
--header 'Request-Id: 1'
```
Ответ
```
rule with GUID and header
```
Подобное поведение необходимо для того, чтобы можно было определить базовое 
правило и далее переопрелить его более частными, если это необходимо.

Поведение можно изменить с помощью переменной среды `AllowMultipleRules`, задав значение 
`false`. В этом случае, если запрос будет удовлетворять нескольким правилам, LIRA
выдаст ошибку с соответствующим описанием.

Запуск и параметры конфигурации описаны в разделе [Запуск](guide.md#запуск).


## Что дальше?
[Генерация ответов](generating.md)

[Сопоставление запросов с помощью интервалов](ranges.md)
## Ссылки
[Полное руководство](guide.md)

[Быстрый старт](../readme.md)
