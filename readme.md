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

Для создания правил добавляем файлы с расширением `.rules` в каталог `c:/rules`.

Все примеры доступны в каталоге `docs/examples`




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
200

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

Ответ
```
200

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
200

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
200

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
200

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
[variables.rules](docs/examples/quick_start/variables.rules)

Значение переменной вычисляется один раз за время обработки запроса. 

Используется для того, чтобы в разные части запроса передавать одно и то же вычисленное динамичекской значение. 

Часто используется при осуществлении обратных вызовов (будут рассмотрены далее).

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
200

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
200

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





#### Вариативность ответов при неизменных данных запроса
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
200

{
    "created_at": "09 Sep 2023 02:36 PM"
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
200

{
    "created_at": "09 Sep 2023 02:36 PM"
}
```

Ссылки

[Полное руководство](docs/guide.md)






## Что дальше?
[Полное руководство](docs/guide.md)
