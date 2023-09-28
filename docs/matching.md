# Cопоставления запросов

**LIRA** позволяет настроить правила по следующим параметрам запроса:
- метод (`GET`, `PUT`, `POST`, `DELETE`, `HEAD`, `TRACE`, `OPTIONS`, `CONNECT`)
- путь 
- параметры строки запроса
- заголовки
- тело

#### Пример
[static.rules](examples/guide/static.rules)
```
-------------------- rule

POST /payment/card?amount=100

~ headers
example: static

~ body
number=1111222233334444

----- response

~ code
200

~ body
static rules matched
```

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

***функция сопосталения*** - одна из [функций сопоставления](docs/match_functions.md)

далее в тексте будет обозначаться как 

`[шаблон сопоставления]`

:triangular_flag_on_post: Шаблоны с несколькими функциями сопоставления не допускаются

#### Примеры

#### Не корректный шаблон сопосталения

`ivanov_{{ any }}_{{ any }}`

#### Корректный шаблон сопосталения

`ivanov_{{ any }}_petrovich`




## Сопоставление пути

Сегмент пути может быть задан либо статично, либо шаблоном сопоставления:

**/**`[[статичная часть 1] | [шаблон сопоставления 1]`**/.../**`[[статичная часть N] | [шаблон сопоставления N]]`

#### Примеры

#### C указанием начального и конечного значения в сегменте
[path_with_start_end.rules](examples/guide/path_with_start_end.rules)
```
-------------------- rule

GET /user/ivanov_{{ any }}_petrovich

----- response

~ code
200

~ body
ivanov user matched
```
Запросы подпадающие под правило
```
curl --location 'http://localhost/user/ivanov_petr_petrovich'
curl --location 'http://localhost/user/ivanov_ivan_petrovich'
```

#### C ипользованием только функции сопоставления
[path_with_one_func.rules](examples/guide/path_with_one_func.rules)
```
-------------------- rule

GET /user/{{ any }}

----- response

~ code
200

~ body
any user matched
```
Запросы подпадающие под правило
```
curl --location 'http://localhost/user/123'
curl --location 'http://localhost/user/ivan_petrovich'
curl --location 'http://localhost/user/df1243'
curl --location 'http://localhost/user/ivan_petrovich'
```

#### C ипользованием функций в разных сегментах
[path_with_two_func.rules](examples/guide/path_with_two_func.rules)
```
-------------------- rule

GET /user/{{ any }}/{{ int }}

----- response

~ code
200

~ body
any user with id matched
```
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
```
-------------------- rule

GET /user?name={{ any }}

----- response

~ code
200

~ body
any user by query param matched
```
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
```
-------------------- rule

GET /user?name=a{{ any }}a

----- response

~ code
200

~ body
user a***a matched
```
Запросы подпадающие под правило
```
curl --location 'http://localhost/user?name=anna'
curl --location 'http://localhost/user?name=alina'
```



#### C ипользованием функций в разных параметрах
[query_with_two_func.rules](examples/guide/query_with_two_func.rules)
```
-------------------- rule

GET /user?name={{ any }}&ago={{ int }}

----- response

~ code
200

~ body
any user with ago matched
```

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
```
-------------------- rule

POST /user

~ headers
Request-Id: {{ guid }}

----- response

~ code
200

~ body
matches by one header by function 
```
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

```
-------------------- rule

POST /user

~ headers
Token: 11{{ any }}22
Request-Time: {{ any }}
No-Cache: true

----- response

~ code
200

~ body
matches by one header by static and functions  
```
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
```
-------------------- rule

POST /user

~ headers
example: body_with_one_func

~ body
{{ int }}

----- response

~ code
200

~ body
matches body by one function
```
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

Синтаксис функции 
```
jpath: <JSON Path выражение>
```

#### Пример

[body_jpath.rules](examples/guide/body_jpath.rules)
```
-------------------- rule

POST /payment/card

~ headers
example: body_jpath

~ body
jpath: $.number >> {{ int }}
jpath: $.owner >> Rodrygo

----- response

~ code
200

~ body
payment by card from Rodrygo. Matched by Json Path
```
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

Синтаксис функции
```
xpath: <XPath выражение>
```

#### Пример

[body_xpath.rules](examples/guide/body_xpath.rules)
```
-------------------- rule

POST /payment/card

~ headers
example: body_xpath

~ body
xpath: /root/number/text() >> {{ int }}
xpath: /root/owner/text() >> Rodrygo

----- response

~ code
200

~ body
payment by card from Rodrygo. Matched by XPath
```
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

Синтаксис функции
```
form: <наименование параметра>
```

#### Пример

[body_form.rules](examples/guide/body_xpath.rules)
```
-------------------- rule

POST /payment/card

~ headers
example: body_form

~ body
form: number >> {{ int }}
form: owner  >> Rodrygo

----- response

~ code
200

~ body
payment by card from Rodrygo. Matched by Form
```
Запрос подпадающий под правило
```
curl --location 'http://localhost/payment/card' \
--header 'example: body_form' \
--header 'Content-Type: application/x-www-form-urlencoded' \
--data-urlencode 'number=1111222233334444' \
--data-urlencode 'owner=Rodrygo'
```


## Сопоставление по условию
Используется при необходимости выдавать разные варианты ответа на один и тот же запрос. 
Условия описываются в секции `condition`. Условия могут быть заданы либо по номеру попытки, 
либо по количеству времени прошедшему с момента первого запроса

В качестве данных, используемых для определения уникальности запроса, используются все данные запроса
(метод, путь, параметры строки запроса, заголовки, тело)

### Сопоставление по номеру попытки

Для сопоставления по номеру попытки используется системная переменная `@attempt`
**Синтаксис секции**
```
[@attempt <условие 1> <номер попытки>]
...
[@attempt <условие N> <номер попытки>]
```

***условие*** - может быть задано одним из операторов `=`,`>`, `>=`,`<`,`<=`,`in`

Оператор `in` определяет интервал, в который должна входить переменная `@attempt`. 
Эквивалентно записи
```
[@attempt >= <верхний интервал>]
[@attempt <= <нижний интервал>]
```

***номер попытки***

#### Пример









!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
```
-------------------- rule

POST /payment/card

~ headers
example: body_xpath

~ body
xpath: /root/number/text() >> {{ int }}
xpath: /root/owner/text() >> Rodrygo

----- response

~ code
200

~ body
payment by card from Rodrygo. Matched by XPath
```

```
-------------------- rule

POST /payment/card

~ headers
example: body_xpath

~ body
xpath: /root/number/text() >> {{ int }}
xpath: /root/owner/text() >> Rodrygo

----- response

~ code
200

~ body
payment by card from Rodrygo. Matched by XPath
```