# Sentence Generator

This is a simple program which uses a Markov chain to generate random sentences.

Feed it a text file with one sentence per line and it will build a Markov chain using each pair of words as a state.
It will then use the resulting Markov chain to generate 1000 random sentences.

Basic usage:

```
SentenceGenerator <training text file> <output text file>
```