# Jedlix

In this application we are taking in a json file then gets deserialized and processed.
With the given json we can do the necessary manipulations to build a charging schedule.

I have created two endpoints, one that returns a json file and another that returns a json string.

The charging charging schedule is mainly based on:

1. Starting time
2. Leaving time
3. Tariffs

- With these we can build a schedule that helps the user determine what are the best times to be charging optimized for the lowest energy bill

Technologies used:

- Mediatr + CQRS
- Swagger for API enpoints

Return types:
- Json file
- Json string 
