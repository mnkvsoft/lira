# Диапазоны

## Введение
Как правило, требуется проверять поведение исходной системы при разных вариантах
ответа от внешней системы. Подобная вариативность настраивается с помощью 
*диапазонов* (**range**) различных типов данных.

Диапазоны могут быть написаны вручную, а может быть использован специальный, 
предназначенный для этого функционал.

Для того, чтобы понять как это работает сначала рассмотрим написание правил 
с ручным определением диапазонов.

Рассмотрим простой пример. 

Есть метод API `POST /payment`, который выполняет перевод денежных
средств с одного счета на другой. Формата обмена сообщениями - `JSON`. 

Метод принимает следующие параметры в теле запроса:
- номер счета списания (`source`)
- номер счета зачисления (`destination`)
- сумму (`amount`)

**Пример**
```
{
    "source": "1",
    "destination": "2",
    "amount": 10.23
}
```

Возвращает следующие данные:
- идентификатор платежа (`id`, целое число)
- статус (`status`). Может принимать следующие значения: `success`, `pending`, `reject`

**Пример**
```
{
    "id": 32,
    "status": "success"
}
```

Напишем простое правило, которое всегда будет возвращать статус `success` 
и последовательное значение идентификатора  

[success_always](examples/guide/ranges/success_always.rules)
```
-------------------- rule

POST /payment

~ headers
example: success_always

----- response

~ body
{
    "id": {{ seq }},
    "status": "success"
}
```
Функция [seq](generating.md#seq) последовательно возвращает целочисленный идентификатор


**Запрос**
```
{
    "source": "1",
    "destination": "2",
    "amount": 10.23
}
```
**Ответ**
```
{
    "id": 32,
    "status": "success"
}
```
Для того, чтобы проверить поведение исходной системы, при различных значениях поля `status`,
необходимо каким-то образом выдавать разные значения. Это можно сделать опираясь на 
входные данные, которые поступают в метод. Возьмем в качестве таких данных поле `amount`
и будет выдавать различные значения поля `status` в зависимости от поля `amount`.
Введем 3 интервала для поля `amount` в зависимости от которого будем выдавать соответствующее 
значение для поля `status`:
- `0.01 - 10.00` - `success`
- `10.01 - 20.00` - `pending`
- `20.01 - 30.00` - `reject`

Напишем соответствующие правила.

[manual_payment](examples/guide/ranges/manual_payment.rules)
```
## success

-------------------- rule

POST /payment

~ headers
example: manual_payment

~ body
{{ jpath: $.amount }} >> {{ dec: [0.01 - 10.00] }}

----- response

~ body
{
    "id": {{ seq }},
    "status": "success"
}

## pending

-------------------- rule

POST /payment

~ headers
example: manual_payment

~ body
{{ jpath: $.amount }} >> {{ dec: [10.01 - 20.00] }}

----- response

~ body
{
    "id": {{ seq }},
    "status": "pending"
}

## reject

-------------------- rule

POST /payment

~ headers
example: manual_payment

~ body
{{ jpath: $.amount }} >> {{ dec: [20.01 - 30.00] }}

----- response

~ body
{
    "id": {{ seq }},
    "status": "reject"
}
```
**Запрос возвращающий статус `success`**
```
curl --location 'http://localhost/payment' \
--header 'example: manual_payment' \
--header 'Content-Type: application/json' \
--data '{
    "source": "1",
    "destination": "2",
    "amount": 5.45
}'
```
**Ответ**
```
{
    "id": 44,
    "status": "success"
}
```
**Запрос возвращающий статус `pending`**
```
curl --location 'http://localhost/payment' \
--header 'example: manual_payment' \
--header 'Content-Type: application/json' \
--data '{
    "source": "1",
    "destination": "2",
    "amount": 15.67
}'
```
**Ответ**
```
{
    "id": 45,
    "status": "pending"
}
```
**Запрос возвращающий статус `reject`**
```
curl --location 'http://localhost/payment' \
--header 'example: manual_payment' \
--header 'Content-Type: application/json' \
--data '{
    "source": "1",
    "destination": "2",
    "amount": 23.89
}'
```
**Ответ**
```
{
    "id": 46,
    "status": "reject"
}
```
Усложним пример. Допустим имеется метод отмены платежа `POST /payment/reversal/<id>`, 
который не принимает в теле сообщения никаких параметров и также возвращает поле `status`- статус отмены платежа.
Поле статус также может содержать одно из трех значений: `success`, `pending`, `reject`.
И также необходимо иметь возможность вернуть все возможные значения. В данном случае для
этого можно использовать только входной параметр `id`.
Введем 3 интервала для параметра `id` в зависимости от которого будем выдавать соответствующее
значение для поля `status`:
- `1 - 1_000_000` - `success`
- `1 000 001  - 2 000 000` - `pending`
- `2 000 001  - 3 000 000` - `reject`

Далее необходимо из метода платежа `POST /payment` возвращать поле `id` из соответствующего 
диапазона. Значение из диапазона `1 - 1_000_000` будем возвращать по умолчанию, а для двух 
других добавим подолнительные диапазоны для поля `amount`:
- `30.01 - 40.00` - вернет `success` для платежа и `id` в интревале `1 000 001  - 2 000 000`
- `40.01 - 50.00` - вернет `success` для платежа и `id` в интревале `2 000 001  - 3 000 000`

Напишем соответствующие правила.

[manual_payment_with_reversal](examples/guide/ranges/manual_payment_with_reversal.rules)
```
## PAYMENT

## success
## Правило №1

-------------------- rule

POST /payment

~ headers
example: manual_payment_with_reversal

~ body
{{ jpath: $.amount }} >> {{ dec: [0.01 - 10.00] }}

----- response

~ body
{
    ## reversal по этому id вернет success
    "id": {{ int: [1 - 1000000] }}, 
    "status": "success"
}


## success with pending reversal
## Правило №2

-------------------- rule

POST /payment

~ headers
example: manual_payment_with_reversal

~ body
{{ jpath: $.amount }} >> {{ dec: [30.01 - 40.00] }}

----- response

~ body
{
    ## reversal по этому id вернет pending
    "id": {{ int: [1000001 - 2000000] }}, 
    "status": "success"
}


## success with reject reversal
## Правило №3

-------------------- rule

POST /payment

~ headers
example: manual_payment_with_reversal

~ body
{{ jpath: $.amount }} >> {{ dec: [40.01 - 50.00] }}

----- response

~ body
{
    ## reversal по этому id вернет reject
    "id": {{ int: [2000001 - 3000000] }}, 
    "status": "success"
}


## pending
## Правило №4

-------------------- rule

POST /payment

~ headers
example: manual_payment_with_reversal

~ body
{{ jpath: $.amount }} >> {{ dec: [10.01 - 20.00] }}

----- response

~ body
{
    ## reversal по этому id вернет success
    "id": {{ int: [1 - 1000000] }}, 
    "status": "pending"
}


## reject
## Правило №5

-------------------- rule

POST /payment

~ headers
example: manual_payment_with_reversal

~ body
{{ jpath: $.amount }} >> {{ dec: [20.01 - 30.00] }}

----- response

~ body
{
    ## reversal по этому id вернет success
    "id": {{ int: [1 - 1000000] }}, 
    "status": "reject"
}

## REVERSAL

## success
## Правило №6

-------------------- rule

POST /payment/reversal/{{ int: [1 - 1000000] }}

~ headers
example: manual_payment_with_reversal

----- response

~ body
{
    "status": "success"
}

## pending
## Правило №7

-------------------- rule

POST /payment/reversal/{{ int: [1000001 - 2000000] }}

~ headers
example: manual_payment_with_reversal

----- response

~ body
{
    "status": "pending"
}

## reject
## Правило №8

-------------------- rule

POST /payment/reversal/{{ int: [2000001 - 3000000] }}

~ headers
example: manual_payment_with_reversal

----- response

~ body
{
    "status": "reject"
}
```
Рассмотрим примеры последовательности вызовов.

### Reversal со статусом success

Для того, чтобы получить статус `success` от метода `/payment/reversal` необходимо, чтобы 
отработало правило `6`, для этого метод `/payment` должен вернуть `id` в диапазоне `1 - 1000000`, 
а для этого необходимо, чтобы отработало одно из правил `1`, `4` или `5`.

Будем использовать правило `1` - для этого передадим в поле `amount` значение в диапазоне `0.01 - 10.00`, 
например `5.91`.

**Вызов**
```
curl --location 'http://localhost/payment' \
--header 'example: manual_payment_with_reversal' \
--header 'Content-Type: application/json' \
--data '{
"source": "1",
"destination": "2",
"amount": 5.91
}'
```
**Ответ**
```
{
    "id": 727119, 
    "status": "success"
}
```
Полученное значение `id` = `727119` передадим в метод `/payment/reversal`
**Вызов**
```
curl --location --request POST 'http://localhost/payment/reversal/727119' \
--header 'example: manual_payment_with_reversal'
```
**Ответ**
```
{
    "status": "success"
}
```
### Reversal со статусом pending

Для того, чтобы получить статус `pending` от метода `/payment/reversal` необходимо, чтобы
отработало правило `7`, для этого метод `/payment` должен вернуть `id` в диапазоне `1000001 - 2000000`,
а для этого необходимо, чтобы отработало правило `2`.
Для этого передадим в поле `amount` значение в диапазоне `30.01 - 40.00`,
например `35.44`.

**Вызов**
```
curl --location 'http://localhost/payment' \
--header 'example: manual_payment_with_reversal' \
--header 'Content-Type: application/json' \
--data '{
    "source": "1",
    "destination": "2",
    "amount": 35.44
}'
```
**Ответ**
```
{
    "id": 1448743, 
    "status": "success"
}
```
Полученное значение `id` = `1448743` передадим в метод `/payment/reversal`

**Вызов**
```
curl --location --request POST 'http://localhost/payment/reversal/1448743' \
--header 'example: manual_payment_with_reversal'
```
**Ответ**
```
{
    "status": "pending"
}
```
Сценарий *Reversal со статусом rejected* делается по аналогии с текущим.

## Использование функционала диапазонов (ranges)

Ручная настройка диапазонов имеет ряд недостатков:
- необходимо отслежить, чтобы диапазоны не пересекались
- необходимо синхронизировать генерацию значений в нужном диапазоне и последующее сопоставление

Для упрощения этих процессов используется функционал диапазонов (ranges).

Диапазоны описываются в файлах `*.ranges.json` и могут располагаться в любой
директории рабочего каталога.

Перепишем пример выше, который был реализован вручную на механизм диапазонов.

Опишем неоходимые диапазоны для методов `/payment` и `/payment/reversal`:

[payment.ranges.json](examples/guide/ranges/payment.ranges.json)
```
{
    "payment.amount": {
      "type": "dec",
      "start": "0.01",
      "capacity": "10",
      "unit": 0.01,
      "ranges": [
        "success",
        "pending",
        "reject",
        "success.reversal.pending",
        "success.reversal.reject"
      ]
    },
    "payment.id": {
        "type": "int",
        "start": "1",
        "capacity": "1_000_000",
        "mode": "seq",
        "ranges": [
          "default",
          "reversal.pending",
          "reversal.reject"
        ]
      }
  }
  
```
Рассмотрим указанный json. 

- `payment.amount`, `reversal.id` - названия интервалов, которые будут использоваться
в правилах, при обращении к ним.

- `type` - тип данных интервала
- `start` - начало интервала
- `capacity` - емкость одного диапазона
- `ranges` - названия диапазов внутри интервала
- `unit` - единица округления выдаваемых значений. Настройка характерна только для типа `float`
- `mode` - режим получения следующего значения для диапазона.
В режиме `seq` значения для каждого диапазона извлекаются последовательно и не 
повторяются. Настройка характерна для типа `int`

:triangular_flag_on_post: В интервале `payment.id` был введен диапазон `default`, хотя ожидаемо было бы увидеть
`reversal.success`, это сделано для того, чтобы был диапазон, для которого не 
требуется какого-то специфичного поведения и при этом дополнительные методы
(как `/payment/reversal` и которые появятся в будущем) смогут использовать его
для поведения по умолчанию.

Для того, чтобы проверить диапазоны, которые были сгенерированы, 
вызовем системный метод `/sys/range/info`

`curl --location 'http://localhost/sys/range/info'`

Ответ
```
-------- payment.amount

Capacity: 10
Unit: 0.01
Ranges:
success                   [0.01 - 10.00]
pending                   [10.01 - 20.00]
reject                    [20.01 - 30.00]
success.reversal.pending  [30.01 - 40.00]
success.reversal.reject   [40.01 - 50.00]


-------- payment.id

Capacity: 1000000
Mode(manual): seq
Ranges:
default           [1 - 1000000]
reversal.pending  [1000001 - 2000000]
reversal.reject   [2000001 - 3000000]

...
```
Видно, что разбиение на диапазоны получилось таким же, как и в ручном режиме.

Опишем правила, которые ранее были написаны в ручном режиме [manual_payment_with_reversal](examples/guide/ranges/manual_payment_with_reversal.rules)
с помощью диапазонов

[ranges_payment_with_reversal](examples/guide/ranges/ranges_payment_with_reversal.rules)
```
## PAYMENT

## success
## Правило №1

-------------------- rule

POST /payment

~ headers
example: ranges_payment_with_reversal

~ body
{{ jpath: $.amount }} >> {{ range: payment.amount/success }}

----- response

~ body
{
    ## reversal по этому id вернет success
    "id": {{ range: payment.id/default }}, 
    "status": "success"
}


## success with pending reversal
## Правило №2

-------------------- rule

POST /payment

~ headers
example: ranges_payment_with_reversal

~ body
{{ jpath: $.amount }} >> {{ range: payment.amount/success.reversal.pending }}

----- response

~ body
{
    ## reversal по этому id вернет pending
    "id": {{ range: payment.id/reversal.pending }}, 
    "status": "success"
}


## success with reject reversal
## Правило №3

-------------------- rule

POST /payment

~ headers
example: ranges_payment_with_reversal

~ body
{{ jpath: $.amount }} >> {{ range: payment.amount/success.reversal.reject }}

----- response

~ body
{
    ## reversal по этому id вернет reject
    "id": {{ range: payment.id/reversal.pending  }}, 
    "status": "success"
}


## pending
## Правило №4

-------------------- rule

POST /payment

~ headers
example: ranges_payment_with_reversal

~ body
{{ jpath: $.amount }} >> {{ range: payment.amount/pending }}

----- response

~ body
{
    ## reversal по этому id вернет success
    "id": {{ range: payment.id/default }}, 
    "status": "pending"
}


## reject
## Правило №5

-------------------- rule

POST /payment

~ headers
example: ranges_payment_with_reversal

~ body
{{ jpath: $.amount }} >> {{ range: payment.amount/pending }}

----- response

~ body
{
    ## reversal по этому id вернет success
    "id": {{ range: payment.id/default }}, 
    "status": "reject"
}

## REVERSAL

## success
## Правило №6

-------------------- rule

POST /payment/reversal/{{ range: payment.id/default }}

~ headers
example: ranges_payment_with_reversal

----- response

~ body
{
    "status": "success"
}

## pending
## Правило №7

-------------------- rule

POST /payment/reversal/{{ range: payment.id/reversal.pending }}

~ headers
example: ranges_payment_with_reversal

----- response

~ body
{
    "status": "pending"
}

## reject
## Правило №8

-------------------- rule

POST /payment/reversal/{{ range: payment.id/reversal.reject }}

~ headers
example: ranges_payment_with_reversal

----- response

~ body
{
    "status": "reject"
}
```
Протестируем написанные правила, убедимся, что результат не отличается от 
ручного написания диапазонов. Проверим сценарий [Reversal со статусом pending](#reversal-со-статусом-pending).  


Для того, чтобы получить статус `pending` от метода `/payment/reversal` необходимо, чтобы
отработало правило `7`, для этого метод `/payment` должен вернуть `id` из диапазона `payment.id/reversal.pending`,
а для этого необходимо, чтобы отработало правило `2`.
Для этого передадим в поле `amount` значение из диапазона `payment.amount/success.reversal.pending` (`30.01 - 40.00`),
например `35.44`.

**Вызов**
```
curl --location 'http://localhost/payment' \
--header 'example: ranges_payment_with_reversal' \
--header 'Content-Type: application/json' \
--data '{
    "source": "1",
    "destination": "2",
    "amount": 35.44
}'
```
**Ответ**
```
{
    "id": 1000001, 
    "status": "success"
}
```
Полученное значение `id` = `1000001` передадим в метод `/payment/reversal`

**Вызов**
```
curl --location --request POST 'http://localhost/payment/reversal/1000001' \
--header 'example: ranges_payment_with_reversal'
```
**Ответ**
```
{
    "status": "pending"
}
```

В файле [payment.ranges.json](examples/guide/ranges/payment.ranges.json) мы вручную
задали параметры разбиения на диапазоны, но можно было бы позволить **LIRA** 
автоматически создать нужные диапазоны

[short.payment.ranges.json](examples/guide/ranges/short.payment.ranges.json)
```
{
    "short.payment.amount": {
      "type": "dec",
      "ranges": [
        "success",
        "pending",
        "reject",
        "success.reversal.pending",
        "success.reversal.reject"
      ]
    },
    "short.payment.id": {
        "type": "int",
        "ranges": [
          "default",
          "reversal.pending",
          "reversal.reject"
        ]
    }
}
```

Префикс *short* был использован, чтобы не было конфликта 
с ранее определенными интервалами. 

Вызовем системный метод `/sys/range/info` для того, чтобы посмотреть какие диапазоны
**LIRA** сгенерировал автоматически: 

`curl --location 'http://localhost/sys/range/info'`

Ответ
```
...

-------- short.payment.amount

Interval(default) : [0.01 - 1000000]
Capacity(auto): 200000.00
Unit:(default) 0.01
Ranges:
success                   [0.01 - 200000.00]
pending                   [200000.01 - 400000.00]
reject                    [400000.01 - 600000.00]
success.reversal.pending  [600000.01 - 800000.00]
success.reversal.reject   [800000.01 - 1000000]


-------- short.payment.id

Interval(default): [1 - 9223372036854775807]
Capacity(auto): 3074457345618258602
Mode(default): seq
Ranges:
default           [1 - 3074457345618258602]
reversal.pending  [3074457345618258603 - 6148914691236517204]
reversal.reject   [6148914691236517205 - 9223372036854775807]

...
```
В этом случае сервер берет для интервала значение по умолчанию, а для вычисления
`capacity` делит это значение на количество диапазонов, т.е. в этом случае
значения диапазонов меняются при изменении их количества.

При таком подходе диапазоны получаются максимальными, но есть недостаток - нельзя сохранить где-то значения диапазонов и потом
использовать их, т.к. границы диапазонов будут меняться при добавлении новых значений.

Чтобы решить эту проблему реализован системный метод 
`/sys/range/val/<имя_интервала>/<имя_диапазона>/[количество_значений]`, который возвращает список
значений из указанного диапазона.

Получим 5 значений для интервала `short.payment.id` и диапазона `success`

**Вызова**
```
curl --location 'http://localhost/sys/range/val/short.payment.amount/success/5'
```
**Ответ**
```
139699.58
122185.09
69056.83
107450.36
98495.50
```

## Системные функции для работы с интервалами

`/sys/range/info` - возвращает информацию о всех зарегистрированных интервалах

`/sys/range/info/<имя_интервала>` - возвращает информацию об указанном интервале

`/sys/range/val/<имя_интервала>/<имя_диапазона>/[количество_значений]` - 
возвращает список значений для указанного диапазона, 
если параметр ***количество_значений*** не указан, то успользуется значение `20`

## Описание интервалов

Формат описания файлов `*.ranges.json`:

```
{
  "<имя_интервала_1>": {
    "type": "<тип_интервала>",
    [специфичные_параметры_для_типа]
    "ranges": [
      "<имя_диапазона_1>",
      ...
      "<имя_диапазона_N>"
    ]
  }
  ...
  "<имя_интервала_N>": {
    "type": "<тип_интервала>",
    [специфичные_параметры_для_типа]
    "ranges": [
      "<имя_диапазона_1>",
      ...
      "<имя_диапазона_N>"
    ]
  }
}
```

Ниже рассмотрим все доступные типы

## int

Для данного типа существуют следующие способы генерации диапазонов:
- по заданному интервалу значений. В этом случае указывается узел `interval`. 
При таком способе генерации диапазонов их емкости получаются максимальными, 
но границы диапазонов изменяются при добавлении новых (диапазонов).

- по заданному стартовому значению интервала и емкости каждого диапазона. 
В этом случае указываются узлы `start`, `capacity`. При таком способе генерации
диапазонов их емкости остаются постоянными, а увеличивается общий интервал 
значений занятый всеми диапазонами.

Если не указан ни один из способов генерации значений, то по умолчанию используется
генерация по интервалу значений `1 - 9223372036854775807`

#### Синтаксис
```
{
  "<имя_интервала>": {
    "type": "int",
    [
      ["interval": "<интервал>",] | 
      [
        <"start": "<начало_интервала>",>
        <"capacity": "<емкость_диапазона>",>
      ]
    ]
    ["mode": "<режим_выдачи_значений>",]
    "ranges": [
      "<диапазон_1>",
      ...
      "<диапазон_N>",
    ]
  }
}
```

***интервал*** - интервал значений в формате `[начало_интервала] - [конец_интервала]`. 
Для обозначения начала и конца интервала могут быть использованы [симпатичные числа](guide.md#симпатичные-числа).

***начало_интервала*** - начало интервала значений. В качестве значения может
быть использовано [симпатичное число](guide.md#симпатичные-числа). При использовании
этого параметра обязателен параметр _емкость_диапазона_.

***емкость_диапазона*** - емкость диапазона, число. В качестве значения может 
быть использовано [симпатичное число](guide.md#симпатичные-числа)

***режим_выдачи_значений*** - может принимать следующие значения:

`seq` - для каждого диапазона значения выдаются последовательно и не повторяются
`random` - для каждого диапазона значения выдаются случайным образом и могут повторяться

Если значение не указано, то используется режим `seq`.

#### Пример с указанием всех настроек для герерации значений по интервалу
```
{
    "id": {
        "type": "int",
        "interval": "1 - 1_000_000_000_000",
        "mode": "random",
        "ranges": [
          "first",
          "second",
          "third"
        ]
    }
}
```

#### Пример с указанием всех настроек для герерации значений по емкости диапазона
```
{
    "id": {
        "type": "int",
        "start": "1",
        "capacity": "1_000",
        "mode": "random",
        "ranges": [
          "first",
          "second",
          "third"
        ]
    }
}
```

#### Пример с настройками по умолчанию в явном виде
```
{
    "id": {
        "type": "int",
        "interval": "1 - 9_223_372_036_854_775_807",
        "mode": "seq",
        "ranges": [
          "first",
          "second",
          "third"
        ]
    }
}
```
#### Пример с настройками по умолчанию без их явного указания
```
{
    "id": {
        "type": "int",
        "ranges": [
          "first",
          "second",
          "third"
        ]
    }
}
```




## float

Для данного типа существуют следующие способы генерации диапазонов:
- по заданному интервалу значений. В этом случае указывается узел `interval`.
  При таком способе генерации диапазонов их емкости получаются максимальными,
  но границы диапазонов изменяются при добавлении новых (диапазонов).

- по заданному стартовому значению интервала и емкости каждого диапазона.
  В этом случае указываются узлы `start`, `capacity`. При таком способе генерации
  диапазонов их емкости остаются постоянными, а увеличивается общий интервал
  значений занятый всеми диапазонами.

Если не указан ни один из способов генерации значений, то по умолчанию используется
генерация по интервалу значений `<минимальная_единица> - 1_000_000`

#### Синтаксис
```
{
  "<имя_интервала>": {
    "type": "dec",
    ["unit": "<единица_округления>",]
    [
      ["interval": "<интервал>",] | 
      [
        <"start": "<начало_интервала>",>
        <"capacity": "<емкость_диапазона>",>
      ]
    ]
    "ranges": [
      "<диапазон_1>",
      ...
      "<диапазон_N>",
    ]
  }
}
```

***единица_округления*** - единица округления выдаваемых значений.

Если значение не указано, то используется значение `0.01`.

***интервал*** - интервал значений в формате `[начало_интервала] - [конец_интервала]`.
Для обозначения начала и конца интервала могут быть использованы [симпатичные числа](guide.md#симпатичные-числа).

***начало_интервала*** - начало интервала значений. В качестве значения может
быть использовано [симпатичное число](guide.md#симпатичные-числа). При использовании
этого параметра обязателен параметр _емкость_диапазона_.

***емкость_диапазона*** - емкость диапазона, число. В качестве значения может
быть использовано [симпатичное число](guide.md#симпатичные-числа)

#### Пример с указанием всех настроек для герерации значений по интервалу
```
{
  "amount": {
    "type": "dec",
    "unit": 0.001,
    "interval": "1 - 100",
    "ranges": [
      "first",
      "second",
      "third"
    ]
  }
}
```
#### Пример с указанием всех настроек для герерации значений по емкости диапазона
```
{
  "amount": {
    "type": "dec",
    "unit": 0.01,
    "start": "0.01",
    "capacity": "1k",
    "ranges": [
      "first",
      "second",
      "third"
    ]
  }
}
```

#### Пример с настройками по умолчанию в явном виде
```
{
  "amount": {
    "type": "dec",
    "unit": 0.01,
    "interval": "1 - 1_000_000",
    "ranges": [
      "first",
      "second",
      "third"
    ]
  }
}
```

#### Пример с настройками по умолчанию без их явного указания
```
{
  "amount": {
    "type": "dec",
    "ranges": [
      "first",
      "second",
      "third"
    ]
  }
}
```





## guid

:triangular_flag_on_post: Получение значений для этого типа возможно только с 
помощью системной функции [/sys/range/val/...](#системные-функции-для-работы-с-интервалами)

```
{
  "<имя_интервала>": {
    "type": "guid",
    ["format": "<формат>",]
    "ranges": [
      "<диапазон_1>",
      ...
      "<диапазон_N>",
    ]
  }
}
```
***формат*** - формат выдачи значения. [О форматах GUID](https://learn.microsoft.com/ru-ru/dotnet/api/system.guid.tostring?view=net-7.0) 

Если значение не указано, то используется формат `00000000-0000-0000-0000-000000000000`

#### Пример с указанием всех настроек
```
{
  "id": {
    "type": "guid",
    "format": "N", 
    "ranges": [
      "first",
      "second",
      "third"
    ]
  }
}
```
#### Пример с настройками по умолчанию в явном виде
```
{
  "id": {
    "type": "guid",
    "format": "D",
    "ranges": [
      "first",
      "second",
      "third"
    ]
  }
}
```
#### Пример с настройками по умолчанию без их явного указания
```
{
  "id": {
    "type": "guid",
    "ranges": [
      "first",
      "second",
      "third"
    ]
  }
}
```







## hex

Функций используется при необходимости сформировать диапазоны для таких данных как
токены, другие данные представленные в виде строк в шестнадцатеричном 
формате, либо, если нужны просто длинные строки используемые в качестве диапазонов.

:triangular_flag_on_post: Получение значений для этого типа возможно только с
помощью системной функции [/sys/range/val/...](#системные-функции-для-работы-с-интервалами)

```
{
  "<имя_интервала>": {
    "type": "hex",
    ["bytes_count": "<количество_байт>",]
    "ranges": [
      "<диапазон_1>",
      ...
      "<диапазон_N>",
    ]
  }
}
```

***количество_байт*** - количество байт используемых для формирования 
шестнадцатеричной строки. Минимальное значение `8`

Если значение не указано, то используется значение `32`

#### Пример с указанием всех настроек
```
{
  "token": {
    "type": "hex",
    "bytes_count": 64,
    "ranges": [
      "first",
      "second",
      "third"
    ]
  }
}
```
#### Пример с настройками по умолчанию в явном виде
```
{
  "token": {
    "type": "hex",
    "bytes_count": 32,
    "ranges": [
      "first",
      "second",
      "third"
    ]
  }
}
```
#### Пример с настройками по умолчанию без их явного указания
```
{
  "token": {
    "type": "hex",
    "ranges": [
      "first",
      "second",
      "third"
    ]
  }
}
```

