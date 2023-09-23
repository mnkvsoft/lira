# Функции сопоставления
Каждая функция сопоставления может использоваться во всех данных запроса (путь, параметры строки запроса, заголовки, тело), кроме названия http - метода.

Рассмотрим на примере функции `any`

- путь запроса

```http request
-------------------- rule

GET /user/{{ any }}

----- response

code:
200

body:
matched by path!
```

- параметр строки запроса

```
-------------------- rule

GET /user/nikolas?lastname={{ any }}

----- response

code:
200

body:
matched by query parameter!
```

- заголовок

```
-------------------- rule

GET /user/nikolas

headers:
Token: {{ any }}

----- response

code:
200

body:
matched by header!
```

- тело

```
-------------------- rule

GET /user/nikolas

body:
{{ any }}

----- response

code:
200

body:
matched by body!
```

Ниже описаны доступные функции сопоставления.


`any` - соответствует любой строке

---

`int` - целое число в диапазоне `- 9 223 372 036 854 775 808` - `9 223 372 036 854 775 807` (64 бита)

---

`guid` - GUID в любом формате

---

`regex: <регулярное_выражение>` - соотвествует регулярному выражению указанному в аргументе

Пример:
```http request
-------------------- rule

GET /user/{{ regex: \d\d\d }}

----- response

code:
200

body:
matche by regex!
```

---

`data: < имя_данных >.< диапазон >` - проверяет строку на принадлежность диапазону определенному в файле `*.data.json`. 

Данная функция используется в сложных сценариях сопоставления, где необходимо выдавать разные ответы при запросе на одну и ту же конечную точку. Работа с функцией описана в [отдельном разделе](data.md)