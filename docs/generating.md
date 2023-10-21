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

## Предопреденные системные функции
Системные функции делятся на 2 группы:
- извлекающие данные запроса
- генерирующие данные

Ниже рассмотрены обе группы

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

Все сгенерированные данные данные могут быть отформатированы, 
если возвращенный тип поддерживает форматирование.

Функции геренерирующие даты по умолчанию возвращают данные в формате ISO_8601. 
Подробнее с форматом можно ознакомиться по [ссылке](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#Roundtrip)

## now
Возвращает текущую дату в локальном времени

## now.utc
Возвращает текущую дату в UTC

## date
Возвращает текущую дату в локальном времени в интервале `[текущая дата - 10 лет] - [текущая дата]` 

## date.utc
Возвращает текущую дату в UTC в интервале `[текущая дата - 10 лет] - [текущая дата]`

## float
Возвращает число с 2 знаками после запятой в интервале `0.01 - 10000`

## guid 
Возвращает GUID. Формат по умолчанию: `aaaaaaaa-bbbb-cccc-dddd-ffffffffffff`

## int
Возвращает целочисленное значение

#### Синтаксис
```
int
```

- пользовательские функции
- переменные
- блоки кода на языке C#