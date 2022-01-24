const {JSONPath} = require('jsonpath-plus');

export const xpath_getter = (data, xpath)=>{
    const result = JSONPath({path: xpath, json: data});
    return result[0]
}
