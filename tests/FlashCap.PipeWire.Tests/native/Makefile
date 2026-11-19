CC = gcc
CPPFLAGS += -D_GNU_SOURCE $(shell pkg-config --cflags libpipewire-0.3 gio-unix-2.0)
CFLAGS += -std=c99 -Wall -Wextra -Werror -Wno-unused-parameter -g
LDLIBS += $(shell pkg-config --libs libpipewire-0.3 gio-unix-2.0)

.PHONY: all clean
all: fixture

fixture: fixture.c
	$(CC) $(CPPFLAGS) $(CFLAGS) -o $@ $< $(LDLIBS)

clean:
	rm -f fixture
