# LIRA
simple mock server

## Миссия
Сервер **LIRA** предназначен для подмены внешних api при тестиронии исходной системы. 
Главной целью сервера **LIRA** является максимально простая настройка и обеспечение широкого 
спектра возможностей

## Отличительные особенности 
Все правила настраиваются с помощью файлов специльного формата (`*.rules`), 
который позволяет настраивать выдаваемые в ответе данные без дополнительного 
экранирования, как это может потребоваться при использовании в качестве настроек
файлов в `json` формате или настройки путем вызова `http` методов сервера.


## Требования 
Установленный Docker

## Ссылки

[Полное руководство](docs/guide.md)

[Cопоставление запросов](docs/matching.md)

[Генерация ответов](docs/generating.md)

[Диапазоны](docs/ranges.md)

## Быстрый старт

### Запуск

- создать каталог `c:/rules` в котором будут находится правила для **LIRA** (можно задать любой другой каталог)
- выполнить команду:

`docker run -p 80:80 -e TZ=Europe/Moscow -v c:/rules:/app/rules  mnkvsoft/lira`

Убедиться, что сервер запущен можно перейдя в браузере по адресу:

`http://localhost`

если сервер работает, то браузер выведет текст: *Rule not found*

### Добавление первого правила
Необходимо создать в каталоге `c:/rules` текстовый файл `hello.rules` следующего содержания:

[hello.rules](docs/examples/quick_start/hello.rules)

```
-------------------- rule

GET /hello/{{:person  any  }}

----- response

~ code
200

~ body
hello {{ req.path: person }}!
```

Тестируем первое правило в браузере, выполнив запрос к ресурсу 

`http://localhost/hello/Nikolas`

сервер нам выдаст ответ с http-кодом `200` и телом ответа `hello Nikolas!`

```
200

hello Nikolas!
```
#### Ссылки

[Полное руководство](docs/guide.md)

### Подсветка синтаксиса

Для работы с правилами рекомендуется использовать Visual Studio Code, 
с установленным расширением для подсветки синтаксиса

![screenshot](docs/highlight_syntax.png)

[Инструкцию по установке расширения](docs\highlight_setup.md)

### Примеры правил

Для создания правил добавляем файлы с расширением `.rules` в каталог `c:/rules`

Все примеры доступны в каталоге `docs/examples`

:triangular_flag_on_post: Для некоторых примеров используется заголовок `example:[название примера]`,
чтобы не было пересечения с правилами из других примеров. 
На него в примерах не нужно фокусировать внимание

:triangular_flag_on_post: Для облегчения чтения, если в примере ответа http-код не указан, то подразумевается код `200`

### Статичное правило
[static.rules](docs/examples/quick_start/static.rules)

```
-------------------- rule

GET /hi

----- response

~ code
200

~ body
hello!
```

Запрос
```
curl --location 'http://localhost/hi'
```

Ответ
```
hello!
```




### Задержка ответа
[delay.rules](docs/examples/quick_start/delay.rules)

```
-------------------- rule

GET /delay

----- response

~ delay
2000 ms

~ code
200

~ body
long query
```

Запрос
```
curl --location 'http://localhost/delay'
```

Ответ с задержкой в 2000 миллисекунд
```
long query
```




### Динамическое сопоставление параметров запроса
[match_dynamic.rules](docs/examples/quick_start/match_dynamic.rules)

```
-------------------- rule

POST /payment/{{ any }}?fast={{ any }}

~ headers
Request-Id: {{ guid }}

~ body
jpath: $.number >> 4444{{ int }}

----- response

~ code
200

~ body
{
    "id": 12345,
    "status": "ok"
}
```
Запрос
```
curl --location 'http://localhost/payment/card?fast=true' \
--header 'Request-Id: 60a37f8e-0c7f-4404-8dda-64eaf6d13e6a' \
--header 'Content-Type: application/json' \
--data '{
    "number": 4444111122223333
}'

```

Ответ
```
{
    "id": 12345,
    "status": "ok"
}
```

#### Ссылки

[Полное руководство](docs/guide.md)

[Функции сопоставления](docs/match_functions.md)




### Динамическая генерация ответов
[generation_dynamic.rules](docs/examples/quick_start/generation_dynamic.rules)

```
-------------------- rule

GET /order

----- response

~ code
200

~ headers
Request-Time: {{ now >> format: H:mm:ss }}

~ body
{
    "id": {{ int }},
    "amount": {{ float }},
    "transaction_id": "{{ guid }}",
    "created_at": "{{ date >> format: yyyy-MM-dd HH:mm:ss }}",
    "customer": "{{ str }}"
}
```
Запрос
```
curl --location 'http://localhost/order'
```

Ответ
```
Request-Time: 12:07:16

{
    "id": 696462653,
    "amount": 2855.18,
    "transaction_id": "f61790a8-4404-475c-914c-dfecd613f92c",
    "created_at": "2014-04-26 01:05:45",
    "customer": "38g771rh99py7rgrucij"
}
```

#### Ссылки

[Полное руководство](docs/guide.md)

[Функции генерации](docs/generation_functions.md)



### Извлечение параметров запроса
[extract_request_data.rules](docs/examples/quick_start/extract_request_data.rules)

```
-------------------- rule

POST /payment/{{:tool  any  }}?fast={{ any }}

~ headers
Id: {{ any }}

~ body
jpath: $.account >> {{ any }}

----- response

~ code
200

~ headers
Request-Id: {{ req.header: Id}}

~ body
{
    "tool": "{{ req.path: tool }}",
    "is_fast": "{{ req.query: fast }}",
    "account": "{{ req.body.jpath: $.account }}"
}
```
Запрос
```
curl --location 'http://localhost/payment/account?fast=false' \
--header 'Id: 987' \
--header 'Content-Type: application/json' \
--data '{
    "account": "9876543210"
}'
```

Ответ
```
Request-Id: 987

{
    "tool": "account",
    "is_fast": "false",
    "account": "9876543210"
}
```

#### Ссылки

[Полное руководство](docs/guide.md)

[Функции генерации](docs/generation_functions.md)



### Использование переменных
Значение переменной вычисляется один раз за время обработки запроса. 

Используется для того, чтобы в разные части запроса передавать одно и то же вычисленное динамическое значение. 

Часто используется при осуществлении обратных вызовов (будут рассмотрены далее).

[variables.rules](docs/examples/quick_start/variables.rules)
```
-------------------- rule

POST /payment

~ headers
example: variables

----- declare

$$requestId = {{ guid }}

----- response

~ code
200

~ headers
Request-Id: {{ $$requestId >> format: N }}

~ body
{
    "request_id": "{{ $$requestId }}"
}
```
Запрос
```
curl --location --request POST 'http://localhost/payment' \
--header 'example: variables'
```

Ответ
```
Request-Id: 1cfc7bc5ea6146a79dc2820fe7c6c63c

{
    "request_id": "1cfc7bc5-ea61-46a7-9dc2-820fe7c6c63c"
}
```

#### Ссылки

[Полное руководство](docs/guide.md)




### Обратные вызовы
[call.rules](docs/examples/quick_start/call.rules)

```
-------------------- rule

POST /payment

~ headers
example: call

----- declare

$$id = {{ seq }}

----- response

~ code
200

~ body
{
    "id": {{ $$id }}, 
    "status": "pending"
}

----- action.call.http

POST http://localhost/api/callback

~ delay 
100 ms

~ headers
Content-Type: application/json

~ body
{
    "id": {{ $$id }}, 
    "status": "ok"
}


-------------------- rule

POST /api/callback

----- response

~ code
200
```
Запрос
```
curl --location --request POST 'http://localhost/payment' \
--header 'example: call'
```

Ответ
```
{
    "id": 338, 
    "status": "pending"
}

```
Обратный вызов
```
POST http://localhost/api/callback
      
Content-Type: application/json; charset=utf-8
Content-Length: 42
      
{
    "id": 338, 
    "status": "ok"
}
```


#### Ссылки

[Полное руководство](docs/guide.md)




### Комментарии
[comments.rules](docs/examples/quick_start/comments.rules)
```
-------------------- rule

GET /comments

----- response

~ code
200

~ body
## single line comment 
###
    it's multiline
    comment
### 
hello!## comment the rest of the line
hello ### comment in the middle of the line ### world!
```
Запрос
```
curl --location 'http://localhost/comments'
```

Ответ
```
hello!
http://hello
```




### Приоритизация правил
Одному запросу может соответствовать несколько правил, 
LIRA выберет наиболее частное (это поведение по умолчанию, оно конфигурируется)

[priority.rules](docs/examples/quick_start/priority.rules)
```
-------------------- rule

GET /priority/{{ any }}

----- response

~ code
200

~ body
rule with ANY

-------------------- rule

GET /priority/{{ guid }}

----- response

~ code
200

~ body
rule with GUID

-------------------- rule

GET /priority/{{ guid }}

~ headers
Request-Id: {{ any }}

----- response

~ code
200

~ body
rule with GUID and header
```
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

### Вариативность ответов

Как правило, требуется проверять поведение исходной системы при разных вариантах 
ответа от внешней системы. Подобная вариативность настраивается с помощью *диапазонов* (**range**).
Диапазоны описываются в файлах `*.ranges.json`


#### Вариативность ответов. Простой пример

В зависимости от суммы платежа будем выдавать разный статус: `ok` или `reject`

Файл `global.ranges.json`

```json
{
    "amount": {
      "type": "float",
      "ranges": [
        "ok",
        "reject"
      ]
    }
} 
```

[ranges.easy.rules](docs/examples/quick_start/ranges.easy.rules)

```
-------------------- rule

POST /payment

~ headers
example: range.easy

~ body
jpath: $.amount >> {{ range: amount/ok }}

----- response

~ code
200

~ body
{
    "status": "ok"
}

-------------------- rule

POST /payment

~ body
jpath: $.amount >> {{ range: amount/reject }}

----- response

~ code
200

~ body
{
    "status": "reject"
}
```

Получаем значение поля `amount` для получения ответа со статусом `ok`

Запрос
```
curl --location 'http://localhost/sys/range/val/amount/ok/1'
```

Ответ
```
201534.92
```

Использует значение `201534.92` полученное в ответе для выполнения запроса

```
curl --location 'http://localhost/payment' \
--header 'example: range.easy' \
--header 'Content-Type: application/json' \
--data '{
    "amount": 201534.92
}'
```
Ответ
```
{
    "status": "ok"
}
```
Для получения ответа со статусом `reject` запрашимаем значение `amount` для него
```
curl --location 'http://localhost/sys/range/val/amount/reject/1'
```
Ответ
```
401386.54
```
Использует значение `401386.54` полученное в ответе для выполнения запроса
```
curl --location 'http://localhost/payment' \
--header 'example: range.easy' \
--header 'Content-Type: application/json' \
--data '{
    "amount": 401386.54
}'
```
Ответ
```
{
    "status": "reject"
}
```





#### Вариативность ответов. Усложненный пример

Предположим, что при выполнеии платежа мы возвращаем в тестируемую систему его 
идентификатор `payment_id` и далее используя этот идентификатор выполняем возврат
платежа (*refund*). Нам нужно обеспечить два разных ответа на возврат платежа: 
`ok` или `reject`. 

Рассмотрим как это настраивается с помощью диапазонов

Изменим файл [global.ranges.json](docs/examples/quick_start/global.ranges.json) следующим образом

```json
{
    "amount": {
      "type": "float",
      "ranges": [
        "ok",
        "reject",
        "refund_reject"
      ]
    },
    "payment_id": {
      "type": "int",
      "ranges": [
        "ok",
        "refund_reject"
      ]
    }
  }
  
```

[ranges.medium.rules](docs/examples/quick_start/ranges.medium.rules)

```
## ok refund rule

-------------------- rule

POST /payment

~ headers
example: range.medium

~ body
jpath: $.amount >> {{ range: amount/ok }}

----- response

~ code
200

~ body
{
    "id": {{ range: payment_id/ok }},
    "status": "ok"
}

-------------------- rule

POST /payment/refund/{{ range: payment_id/ok}}

~ headers
example: range.medium

----- response

~ code
200

~ body
{
    "status": "ok"
}

## reject refund rule

-------------------- rule

POST /payment

~ headers
example: range.medium

~ body
jpath: $.amount >> {{ range: amount/refund_reject }}

----- response

~ code
200

~ body
{
    "id": {{ range: payment_id/refund_reject }},
    "status": "ok"
}

-------------------- rule

POST /payment/refund/{{ range: payment_id/refund_reject}}

~ headers
example: range.medium

----- response

~ code
200

~ body
{
    "status": "reject"
}
```

***Сценарий получения статуса `ok` на возврат***

Получаем значение поля `amount` для получения ответа со статусом `ok`

Запрос
```
curl --location 'http://localhost/sys/range/val/amount/ok/1'
```

Ответ
```
118297.44
```

Использует значение `118297.44` полученное в ответе для выполнения запроса

```
curl --location 'http://localhost/payment' \
--header 'example: range.medium' \
--header 'Content-Type: application/json' \
--data '{
    "amount": 118297.44
}'
```
Ответ
```
{
    "id": 1,
    "status": "ok"
}

```
Используем полученное значение `id` для выполнения возврата платежа
```
curl --location --request POST 'http://localhost/payment/refund/1' \
--header 'example: range.medium'
```
Ответ
```
{
    "status": "ok"
}

```

***Сценарий получения статуса `reject` на возврат***


Для получения ответа со статусом `reject` на возврат запрашимаем значение `amount` для него
```
curl --location 'http://localhost/sys/range/val/amount/refund_reject/1'
```
Ответ
```
742387.47
```
Использует значение `742387.47` полученное в ответе для выполнения запроса
```
curl --location 'http://localhost/payment' \
--header 'example: range.medium' \
--header 'Content-Type: application/json' \
--data '{
    "amount": 742387.47
}'
```
Ответ
```
{
    "id": 4611686018427387904,
    "status": "ok"
}
```
Используем полученное значение `id` для выполнения возврата платежа
```
curl --location --request POST 'http://localhost/payment/refund/4611686018427387904' \
--header 'example: range.medium'
```
Ответ
```
{
    "status": "reject"
}
```
#### Ссылки

[Диапазоны](docs/ranges.md)

[Полное руководство](docs/guide.md)





### Вариативность ответов при неизменных данных запроса

:triangular_flag_on_post: Если выполнять этот пример в Postman, 
то необходимо отлючить функцию добавления заголовка Postman-Token, 
т.к. в него записывается новое значение при каждом запросе 
и сервер интерпретирует такие запросы как разные

[conditions.rules](docs/examples/quick_start/conditions.rules)
```
---------------------------- rule

GET /payment/status

--------------- condition

@elapsed < 2 second

----- response

~ code
200

~ body
{
    "status": "registered"
}

--------------- condition

@elapsed in [2 second - 4 second]

----- response

~ code
200

~ body
{
    "status": "pending"
}


--------------- condition

@elapsed > 4 second

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
curl --location 'http://localhost/pay/status'
```

Ответы:

С момента первого запроса прошло менее 2 секунд
```
{
    "status": "registered"
}
```
С момента первого запроса прошло от 2 до 4 секунд
```
{
    "status": "pending"
}
```
С момента первого запроса прошло более 4 секунд
```
{
    "status": "ok"
}
```





### Определение собственных функций
[custom_function.rules](docs/examples/quick_start/custom_function.rules)

```
-------------------- rule

POST /payment

~ headers
example: custom_function

----- declare

$payment.now = {{ now >> format: dd MMM yyyy hh:mm tt }}

----- response

~ code
200

~ body
{
    "created_at": "{{ $payment.now }}"
    
    ## the '#' symbol can be omitted when calling a function
    ## "created_at": "{{ payment.now }}"
}
```
Запрос
```
curl --location --request POST 'http://localhost/payment' \
--header 'example: custom_function'
```

Ответ
```
{
    "created_at": "09 Sep 2023 02:36 PM"
}
```

#### Ссылки

[Полное руководство](docs/guide.md)




### Общие переменные и функции
Переменные и функции можно задекларировать на уровне файла с правилами 
и в этом случае переиспользовать в рамках одного файла, 
либо задекларировать на глобальном уровне и использовать в любом правиле

#### Декларирование на уровне файла

[declare.shared.file.rules](docs/examples/quick_start/declare.shared.file.rules)

```
-------------------- declare

$amount = {{ float: [1 - 100] }}

-------------------- rule

GET /payment

~ headers
example: declare.shared.file

----- response

~ code
200

~ body
{
    "amount": {{ amount }}
}

-------------------- rule

GET /account

~ headers
example: declare.shared.file

----- response

~ code
200

~ body
{
    "balance": {{ amount }}
}
```
Запрос
```
curl --location 'http://localhost/payment' \
--header 'example: declare.shared.file'
```
Ответ
```
{
    "amount": 26.81
}
```

Запрос
```
curl --location 'http://localhost/account' \
--header 'example: declare.shared.file'
```
Ответ
```
{
    "balance": 59.26
}
```

#### Ссылки

[Полное руководство](docs/guide.md)


#### Декларирование на глобальном уровне
Декларирование на глобильном уровне производится в файлах `*.declare`

Добавим файл 
[declare.shared.global.declare](docs/examples/quick_start/declare.shared.global.declare)

```
$age = {{ int: [1 - 122]}}
```

Создадим правило

[declare.shared.global.rules](docs/examples/quick_start/declare.shared.global.rules)

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



### Многострочные переменные и функции
Переменные и функции поддерживают многострочные объявления. 
Это может использоваться для создания шаблонов ответов

[multiline_functions.rules](docs/examples/quick_start/multiline_functions.rules)
```
-------------------- declare

$template.order = 
{
    "id": {{ int }},
    "status": "paid",
    "amount": {{ float }},
    "transaction_id": "{{ guid }}",
    "created_at": "{{ date >> format: yyyy-MM-dd HH:mm:ss }}",
    "customer": "{{ str }}"
}

-------------------- rule

POST /order

~ headers
example: multiline_functions

----- response

~ code
200

~ body
{{ template.order }}

-------------------- rule

GET /order

~ headers
example: multiline_functions

----- response

~ code
200

~ body
{{ template.order }}
```
Запрос
```
curl --location --request POST 'http://localhost/order' \
--header 'example: multiline_functions'
```
Ответ
```
{
    "id": 1930876293,
    "status": "paid",
    "amount": 7081.21,
    "transaction_id": "d282e241-6ddc-47a7-bc84-ebc548b6a0c9",
    "created_at": "2015-03-30 07:21:45",
    "customer": "zsiw62s7hdj4vq7iw1xz"
}
```
Запрос
```
curl --location 'http://localhost/order' \
--header 'example: multiline_functions'
```
Ответ
```
{
    "id": 1874972789,
    "status": "paid",
    "amount": 3537.29,
    "transaction_id": "553cca6b-1501-4a45-a548-90d3de60c48d",
    "created_at": "2016-09-12 17:13:58",
    "customer": "fk1f9kw3nwbd01o8tyrs"
}
```



#### Изменение значение узлов json в шаблонах ответов
Часто в шаблонах json ответов (которые представляют собой просто функции, 
генерирующие некоторые значения) необходимо изменить часть данных, 
а остальные оставить без изменений. Это делается с помощью явного определения типа  
возвращаемого функцией значения как `json`. Блок, в котором используется функция является 
блоком `C#` - кода, который будет рассмотрен ниже. 

:triangular_flag_on_post: При вызовах функций в C# - блоках 
символ `$` используемый при объявлении функций не может быть опущен

[change_json.rules](docs/examples/quick_start/change_json.rules)
```
-------------------- declare

$template.order:json = 
{
    "id": {{ int }},
    "status": "paid",
    "amount": {{ float }},
    "transaction_id": "{{ guid }}",
    "created_at": "{{ date >> format: yyyy-MM-dd HH:mm:ss }}",
    "customer": "{{ str }}"
}

-------------------- rule

POST /order

~ headers
example: change_json

----- response

~ code
200

~ body
{{ 
    $template.order
        .replace("$.status", "pending")
        .replace("$.customer", "vasily pupkin")
}}

-------------------- rule

GET /order

~ headers
example: change_json

----- response

~ code
200

~ body
{{ 
    $template.order
        .replace("$.status", "refunded")
        .replace("$.customer", "nikolas john")
}}
```
Запрос
```
curl --location --request POST 'http://localhost/order' \
--header 'example: change_json'
```
Ответ
```
{
  "id": 1753774650,
  "status": "pending",
  "amount": 3528.46,
  "transaction_id": "9dfa5667-9c17-4dbb-9b18-74553c26746e",
  "created_at": "2022-11-18 05:16:21",
  "customer": "vasily pupkin"
}
```
Запрос
```
curl --location 'http://localhost/order' \
--header 'example: change_json'
```
Ответ
```
{
  "id": 1545541359,
  "status": "refunded",
  "amount": 6965.61,
  "transaction_id": "fca022d3-254d-4873-afb7-495996665c26",
  "created_at": "2013-11-19 20:14:38",
  "customer": "nikolas john"
}
```


### Определение динамических блоков на языке C#
В некоторых случаях функциональности встроенных функций не хватает для описания
специфичной логики. В этом случае можно использовать блоки кода на языке C#



#### Короткие блоки
Подразумевают инструцию без использования дополнительных переменных и 
возвращающую **не** `void` значение

[charp.short.rules](docs/examples/quick_start/charp.short.rules)
```
-------------------- rule

GET /very/old/event

----- response

~ code
200

~ body
{
    "date": {{ DateTime.Now.AddYears(-100 - Random.Shared.Next(1, 100)) }}
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



#### Полные блоки
[charp.full.rules](docs/examples/quick_start/charp.full.rules)
```cs
-------------------- rule

POST /payment/card

~ headers
example: csharp.full 

~ body
jpath: $.number >> {{ any }}

----- response

~ code
200

~ body
{
    "mnemonic": {{ 
        string cardNumber = req.body.jpath("$.number");

        string paymentSystem;
        switch(cardNumber[0])
        {
            
            case '2':
                paymentSystem = "MIR";
                break;
            case '4':
                paymentSystem = "VISA";
                break;
            case '5':
                paymentSystem = "MASTERCARD";
                break;
            default:
                paymentSystem = "";
                break;
        }

        string last4 = cardNumber[^4..];
        string result = paymentSystem + " *" + last4;
        return result;
     }}
}
```
Запрос
```
curl --location 'http://localhost/payment/card' \
--header 'example: csharp.full' \
--header 'Content-Type: text/plain' \
--data '{
    "number": "2222333344445678"
}'
```
Ответ
```
{
    "mnemonic": "MIR *5678"
}
```



#### Определение классов
Для совместного использования логики используются файлы `*.cs` с определенными 
в них классами и методами на языке C#. 

Нужно обратить внимание, что в примере, данные для вычисления мнемоники карты, извлекаются из разных 
полей

[CardNumber.cs](docs/examples/quick_start/CardNumber.cs)
```cs
namespace _;

public static class CardNumber
{
    public static string GetMnemonic(string cardNumber)
    {
        string paymentSystem;
        switch(cardNumber[0])
        {
            case '2':
                paymentSystem = "MIR";
                break;
            case '4':
                paymentSystem = "VISA";
                break;
            case '5':
                paymentSystem = "MASTERCARD";
                break;
            default:
                paymentSystem = "";
                break;
        }

        string last4 = cardNumber[^4..];
        string result = paymentSystem + " *" + last4;
        
        return result;
    }
}
```
[charp.class.mnenonic.rules](docs/examples/quick_start/charp.class.mnenonic.rules)
```
-------------------- rule

POST /payment/card

~ headers
example: charp.class.mnenonic

~ body
jpath: $.number >> {{ any }}

----- response

~ code
200

~ body
mnemonic was generated from 'number' field: {{ 
    CardNumber.GetMnemonic(req.body.jpath("$.number")) 
}}


-------------------- rule

POST /payment/card

~ headers
example: charp.class.mnenonic

~ body
jpath: $.pan >> {{ any }}

----- response

~ code
200

~ body
mnemonic was generated from 'pan' field: {{ 
    CardNumber.GetMnemonic(req.body.jpath("$.pan")) 
}}
```
Запрос
```
curl --location 'http://localhost/payment/card' \
--header 'example: charp.class.mnenonic' \
--header 'Content-Type: application/json' \
--data '{
    "number": "2222333344445678"
}'
```
Ответ
```
mnemonic was generated from 'number' field: MIR *5678
```
Запрос
```
curl --location 'http://localhost/payment/card' \
--header 'example: charp.class.mnenonic' \
--header 'Content-Type: application/json' \
--data '{
    "pan": "2222333344445678"
}'
```
Ответ
```
mnemonic was generated from 'pan' field: MIR *5678
```

#### Пример накладывания подписи
[SignatureCalculator.cs](docs/examples/quick_start/SignatureCalculator.cs)
```cs
using System.Security.Cryptography;
using System.Text;

namespace _;

public static class SignatureCalculator
{
    public static string Get(string text, string key)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text + key);
        using var sha1 = SHA1.Create();
        byte[] hash = sha1.ComputeHash(bytes);

        var sb = new StringBuilder(bytes.Length * 2);

        foreach (var b in hash)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }
}

```
[charp.class.sign.rules](docs/examples/quick_start/charp.class.sign.rules)
```
-------------------- rule

POST /payment

~ headers
example: charp.class.sign

----- declare

$response = 
{
    "id" : {{ int }},
    "created_at": "{{ now }}",
    "status": "ok"
}

----- response

~ code
200

~ body
{{
    json($response)
        .add("sign", SignatureCalculator.Get($response, "very_secret_key"))
}}
```
Запрос
```
curl --location --request POST 'http://localhost/payment' \
--header 'example: charp.class.sign'
```
Ответ
```
{
  "id": 911482583,
  "created_at": "09/17/2023 16:19:33",
  "status": "ok",
  "sign": "d4bfbc5b9077d41f7105ad65b047f3f94232d617"
}
```




### Переопределение системных функций
Системные функции могут быть переопределены пользовательскими.
Т.е. если по какой-то причине логика генерации значения системной функции
не подходит, то ее можно заменить своей

В примере ниже фукция `now` генерирует дату без составляющей времени 

[override.rules](docs/examples/quick_start/override.rules)
```
-------------------- rule

GET /order

~ headers
example: override

----- declare

$now = {{ DateTime.Now.ToString("yyyy-MM-dd") }}

----- response

~ code
200

~ body
{
    "created_at": "{{ now }}"
}
```

Запрос
```
curl --location 'http://localhost/order' \
--header 'example: override'
```
Ответ
```
{
    "created_at": "2023-10-08"
}
```


### Форматирование различных типов данных
Форматирование разных типов данных основано на общем механизме форматирования
**.NET** 

Подробное описание форматов для разных типов данных можно найти по 
[ссылке](https://learn.microsoft.com/ru-ru/dotnet/standard/base-types/formatting-types)

Определить тип данных встроенных функций можно следующим правилом

[format_gettype.rules](docs/examples/quick_start/format_gettype.rules)
```
-------------------- rule

GET /get_type

----- response

~ code
200

~ body
{{ guid >> @value.GetType() }}
{{ float >> @value.GetType() }}
{{ date >> @value.GetType() }}
{{ int >> @value.GetType() }}
```
Запрос
```
curl --location 'http://localhost/get_type'
```
Ответ
```
System.Guid
System.Decimal
System.DateTime
System.Int64
```

Форматирование можно осуществить либо использованием встроенной 
функции `format`, либо методом `ToString()` в C# - блоке

[format.rules](docs/examples/quick_start/format.rules)
```
-------------------- rule

GET /format

----- response

~ code
200

~ body

## short function
{{ guid >> format: N }}

## raw C# block
{{ Guid.NewGuid().ToString("D") }}

##  C# block with format function
{{ Guid.NewGuid() >> format: B }}
```

Запрос
```
curl --location 'http://localhost/format'
```
Ответ
```
fccbb0617f8143eebe3a8759570d8859
77f02f06-e021-4565-b95e-9488ad02b9d2
{a740ca3e-0c4c-46b2-9e6b-6bfe77f79da9}
```

## Что дальше?
[Полное руководство](docs/guide.md)
