using NUnit.Framework;
using PredicateBuilder.Test.Data;
using PredicateBuilder.Test.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PredicateBuilder.Test
{
    public class PredicateHelperTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void IsNullableValueTypeIsNullableTest()
        {
            Assert.IsTrue(PredicateHelper.IsNullableValueType(typeof(int?)));
        }

        [Test]
        public void IsNullableValueTypeIsNotNullableTest()
        {
            Assert.IsFalse(PredicateHelper.IsNullableValueType(typeof(int)));
        }

        [Test]
        public void GetNullableUnderlyingTypeTest()
        {
            Assert.AreEqual(typeof(int), PredicateHelper.GetNullableUnderlyingType(typeof(int?)));
        }

        [Test]
        public void GetGenericTypeTest()
        {
            Assert.AreEqual(typeof(int?), PredicateHelper.GetGenericType(typeof(List<int?>)).FirstOrDefault());
        }

        [Test]
        public void PropertyNamePathByExpressionFuncTest()
        {
            string actual = PredicateHelper.PropertyNamePath<PersonDto, string>(x => x.MailingAddress.ISOCountryCode);
            Assert.AreEqual("MailingAddress.ISOCountryCode", actual);
        }

        [Test]
        public void PropertyNamePathByLambdaExpressionTest()
        {
            Expression<Func<PersonDto, string>> propExprFunc = x => x.MailingAddress.ISOCountryCode;
            string actual = PredicateHelper.PropertyNamePath(propExprFunc, typeof(PersonDto));
            Assert.AreEqual("MailingAddress.ISOCountryCode", actual);
        }

        [Test]
        public void GetPropertyLambdaExpression_ExpressionFuncTest()
        {
            Guid guid = Guid.NewGuid();
            PersonDto entity = PersonGenerator.GetPerson(guid);

            Expression<Func<PersonDto, string>> propExprFunc = PredicateHelper.GetPropertyLambdaExpression<PersonDto, string>("MailingAddress.ISOCountryCode");
            Assert.AreEqual(entity.MailingAddress.ISOCountryCode, propExprFunc.Compile()(entity));
        }

        [Test]
        public void GetPropertyLambdaExpression_LambdaExpressionTest()
        {
            Guid guid = Guid.NewGuid();
            PersonDto entity = PersonGenerator.GetPerson(guid);

            LambdaExpression propExprFunc = PredicateHelper.GetPropertyLambdaExpression<PersonDto>("MailingAddress.ISOCountryCode");
            Assert.AreEqual(entity.MailingAddress.ISOCountryCode, ((Func<PersonDto, string>)propExprFunc.Compile())(entity));
        }
    }
}