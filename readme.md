# SimpleMockServer

## Назначение
SimpleMockServer (**SMS**) предназначен для подмены внешних api при тестиронии исходной системы. SMS очень прост в настройке при сохранение максимальной функциональности

## Требования 
Установленный Docker

## Быстрый старт

### Запуск

- создать каталог `c:/rules` в котором будут находится правила для **SMS** (можно задать любой другой каталог)
- выполнить команду:

`docker run -p 80:80  -v c:/rules:/app/rules  mnkvsoft/simplemockserver`

Убедиться, что сервер запущен можно перейдя в браузере по адресу:

`http://localhost`

если сервер работает, то браузер выведет текст: *Rule not found*

### Добавление первого правила
Необходимо создать в каталоге `c:/rules` текстовый файл `hello.rules` следующего содержания:

```
-------------------- rule

GET /hello/{{ any name: person }}

----- response

code:
200

body:
hello {{ req.path: person }}!
```

Тестируем первое правило в браузере, выполнив запрос к ресурсу 

`http://localhost/hello/Nikolas`

сервер нам выдаст ответ с http-кодом 200 и телом ответа: hello world!

```
200

hello Nikolas!
```

### Примеры правил

Для создания правил добавляем файлы с расширением `.rules` в каталог `c:/rules`

Все примеры доступны в каталоге `docs/examples`

Для некоторых примеров используется параметр строки запроса `example=[название пример]`,
чтобы не было пересечения с правилами из других примеров. 
На него в примерах не нужно фокусировать внимание

Ниже, для облегчения чтения, если в примере ответа http-код не указан, то подразумевается код `200`

#### Статичное правило
[static.rules](docs/examples/quick_start/static.rules)

```
-------------------- rule

GET /hi

----- response

code:
200

body:
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




#### Задержка ответа
[delay.rules](docs/examples/quick_start/delay.rules)

```
-------------------- rule

GET /delay

----- response

delay:
2000 ms

code:
200

body:
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




#### Динамическое сопоставление параметров запроса
[match_dynamic.rules](docs/examples/quick_start/match_dynamic.rules)

```
-------------------- rule

POST /pay/{{ any }}?fast={{ any }}

headers:
Request-Id: {{ guid }}

body:
jpath: $.pan >> 4444{{ int }}

----- response

code:
200

body:
{
    "payment_id": 12345,
    "status": "ok"
}
```
Запрос
```
curl --location 'http://localhost/pay/card?fast=true' \
--header 'Request-Id: b35527be-e4b8-458f-8e4e-e4fd241b3454' \
--header 'Content-Type: application/json' \
--data '{
    "pan": 4444111122223333
}'
```

Ответ
```
{
    "payment_id": 12345,
    "status": "ok"
}
```

Ссылки

[Полное руководство](docs/guide.md)

[Функции сопоставления](docs/match_functions.md)




#### Динамическая генерация ответов
[generation_dynamic.rules](docs/examples/quick_start/generation_dynamic.rules)

```
-------------------- rule

GET /order

----- response

code:
200

headers:
Request-Time: {{ now >> format: H:mm:ss }}

body:
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

Ссылки

[Полное руководство](docs/guide.md)

[Функции генерации](docs/generation_functions.md)




#### Извлечение параметров запроса
[generation_dynamic.rules](docs/examples/quick_start/generation_dynamic.rules)

```
-------------------- rule

POST /pay/by/{{ any name: tool }}

----- response

code:
200

headers:
Request-Id: {{ req.header: Id}}

body:
{
    "tool": "{{ req.path: tool }}",
    "is_fast": "{{ req.query: fast }}",
    "account": "{{ req.body.jpath: $.account }}"
}
```
Запрос
```
curl --location 'http://localhost/pay/by/account?fast=false' \
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

Ссылки

[Полное руководство](docs/guide.md)

[Функции генерации](docs/generation_functions.md)




#### Использование переменных
Значение переменной вычисляется один раз за время обработки запроса. 

Используется для того, чтобы в разные части запроса передавать одно и то же вычисленное динамическое значение. 

Часто используется при осуществлении обратных вызовов (будут рассмотрены далее).

[variables.rules](docs/examples/quick_start/variables.rules)
```
-------------------- rule

POST /pay?example=variables

----- declare

$requestId = {{ guid }}

----- response

code:
200

headers:
Request-Id: {{ $requestId }}

body:
{
    "request_id": "{{ $requestId }}"
}
```
Запрос
```
curl --location 'http://localhost/pay?example=variables' \
--header 'Content-Type: application/json' \
--data '{
    "account": "9876543210"
}'
```

Ответ
```
Request-Id: 72bca177-b703-4cc9-8a7f-488908b498a1

{
    "request_id": "72bca177-b703-4cc9-8a7f-488908b498a1"
}
```

Ссылки

[Полное руководство](docs/guide.md)




#### Обратные вызовы
[call.rules](docs/examples/quick_start/call.rules)

```
-------------------- rule

POST /pay?example=call

----- declare

$id = {{ seq }}

----- response

code:
200

body:
{
    "id": {{ $id }}, 
    "status": "pending"
}

----- call.http

POST http://localhost/api/callback

delay: 
100 ms

headers:
Content-Type: application/json

body:
{
    "id": {{ $id }}, 
    "status": "ok"
}


-------------------- rule

POST /api/callback

----- response

code:
200
```
Запрос
```
curl --location --request POST 'http://localhost/pay?example=call'
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


Ссылки

[Полное руководство](docs/guide.md)



!!!!!!!!!!!!!!
Приоритизация правил
Комментарии





#### Вариативность ответов

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

POST /pay?example=range.easy

body:
jpath: $.amount >> {{ range: amount.ok }}

----- response

code:
200

body:
{
    "status": "ok"
}

-------------------- rule

POST /pay

body:
jpath: $.amount >> {{ range: amount.reject }}

----- response

code:
200

body:
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
curl --location 'http://localhost/pay?example=range.easy' \
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
curl --location 'http://localhost/pay?example=range.easy' \
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

[ranges.normal.rules](docs/examples/quick_start/ranges.normal.rules)

```
// ok refund rule

-------------------- rule

POST /pay?example=range.normal

body:
jpath: $.amount >> {{ range: amount.ok }}

----- response

code:
200

body:
{
    "payment_id": {{ range: payment_id.ok }},
    "status": "ok"
}

-------------------- rule

POST /pay/refund/{{ range: payment_id.ok}}

----- response

code:
200

body:
{
    "status": "ok"
}

// reject refund rule

-------------------- rule

POST /pay?example=range.normal

body:
jpath: $.amount >> {{ range: amount.refund_reject }}

----- response

code:
200

body:
{
    "payment_id": {{ range: payment_id.refund_reject }},
    "status": "ok"
}

-------------------- rule

POST /pay/refund/{{ range: payment_id.refund_reject}}

----- response

code:
200

body:
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
curl --location 'http://localhost/pay?example=range.normal' \
--header 'Content-Type: application/json' \
--data '{
    "amount": 118297.44
}'
```
Ответ
```
{
    "payment_id": 1,
    "status": "ok"
}
```
Используем полученное значение `payment_id` для выполнения возврата платежа
```
curl --location --request POST 'http://localhost/pay/refund/1'
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
curl --location 'http://localhost/pay?example=range.normal' \
--header 'Content-Type: application/json' \
--data '{
    "amount": 742387.47
}'
```
Ответ
```
{
    "payment_id": 4611686018427387904,
    "status": "ok"
}
```
Используем полученное значение `payment_id` для выполнения возврата платежа
```
curl --location --request POST 'http://localhost/pay/refund/4611686018427387904'
```
Ответ
```
{
    "status": "reject"
}
```
Ссылки

[Диапазоны](docs/ranges.md)

[Полное руководство](docs/guide.md)





#### Вариативность ответов при неизменных данных запроса

:triangular_flag_on_post: Если выполнять этот пример в Postman, 
то необходимо отлючить функцию добавления заголовка Postman-Token, 
т.к. в него записывается новое значение при каждом запросе 
и сервер интерпретирует такие запросы как разные

[conditions.rules](docs/examples/quick_start/conditions.rules)
```
---------------------------- rule

GET /pay/status

--------------- condition

@elapsed < 2 second

----- response

code:
200

body:
{
    "status": "registered"
}

--------------- condition

@elapsed in [2 second - 4 second]

----- response

code:
200

body:
{
    "status": "pending"
}


--------------- condition

@elapsed > 4 second

----- response

code:
200

body:
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









#### Определение собственных функций
[custom_function.rules](docs/examples/quick_start/custom_function.rules)

```
-------------------- rule

POST /pay?example=custom_function

----- declare

#pay.now = {{ now >> format: dd MMM yyyy hh:mm tt }}

----- response

code:
200

body:
{
    "created_at": "{{ pay.now }}"
}
```
Запрос
```
curl --location --request POST 'http://localhost/pay?example=custom_function'
```

Ответ
```
{
    "created_at": "09 Sep 2023 02:36 PM"
}
```

Ссылки

[Полное руководство](docs/guide.md)






## Что дальше?
[Полное руководство](docs/guide.md)
